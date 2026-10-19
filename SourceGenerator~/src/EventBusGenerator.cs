using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AceLand.EventDriven.SourceGenerator
{
    [Generator]
    public sealed class EventBusGenerator : IIncrementalGenerator
    {
        const string Version = "1.0.0";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Candidate interface declarations. Staying on Roslyn 4.1 (Unity 2022.3),
            // so we use CreateSyntaxProvider and deliberately avoid
            // ForAttributeWithMetadataName (requires Roslyn 4.3.1+).
            var candidates = context.SyntaxProvider
                .CreateSyntaxProvider(IsCandidate, Transform)
                .Collect();

            var input = context.CompilationProvider.Combine(candidates);

            context.RegisterSourceOutput(input, (spc, pair) =>
            {
                try { Produce(spc, pair.Left, pair.Right); }
                catch (Exception e)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(Diags.GeneratorCrashed, Location.None, e.ToString()));
                }
            });
        }

        // ------------------------------------------------------------------ syntax filter

        static bool IsCandidate(SyntaxNode node, CancellationToken _)
            // Only interface declarations can be event interfaces.
            => node is InterfaceDeclarationSyntax { BaseList: not null };

        static InterfaceDeclarationSyntax Transform(GeneratorSyntaxContext ctx, CancellationToken _)
            => (InterfaceDeclarationSyntax)ctx.Node;

        // ------------------------------------------------------------------ production

        static void Produce(SourceProductionContext context, Compilation c,
            ImmutableArray<InterfaceDeclarationSyntax> candidates)
        {
            var symbols = Symbols.TryCreate(c);
            // IBusEvent (and IEvent / IEvent<T>) not referenced → nothing to do.
            if (symbols is null) return;

            var models = new List<EventModel>();
            var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

            foreach (var decl in candidates)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var model = c.GetSemanticModel(decl.SyntaxTree);
                if (model.GetDeclaredSymbol(decl) is not INamedTypeSymbol type) continue;
                if (!seen.Add(type)) continue;

                // Must implement IBusEvent (directly or indirectly), but not be the markers themselves.
                if (!type.ImplementsDirectlyOrIndirectly(symbols.IBusEvent)) continue;
                if (SymbolEqualityComparer.Default.Equals(type, symbols.IEvent)) continue;
                if (SymbolEqualityComparer.Default.Equals(type.ConstructedFrom, symbols.IEventT)) continue;

                var built = Analyze(type, symbols, context);
                if (built != null) models.Add(built);
            }

            // No event interfaces in this assembly → no bootstrap needed.
            if (models.Count == 0) return;

            models.Sort(static (a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));

            // Push model: each assembly registers its own events into the runtime
            // store via a [RuntimeInitializeOnLoadMethod] bootstrap (zero reflection).
            context.AddSource("GeneratedEventBootstrap.g.cs",
                SourceText.From(Emitter.EmitBootstrap(c.AssemblyName, models, Version), System.Text.Encoding.UTF8));
        }

        // ------------------------------------------------------------------ validation

        static EventModel Analyze(INamedTypeSymbol type, Symbols s, SourceProductionContext ctx)
        {
            var display = type.ToDisplayString();
            var loc = type.Locations.FirstOrDefault() ?? Location.None;

            // Open generic event interfaces are unsupported (mirrors runtime behaviour).
            if (type.IsGenericType)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diags.OpenGenericUnsupported, loc, display));
                return null;
            }

            var methods = type.OrdinaryMethods().ToList();
            if (methods.Count != 1)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diags.MustDeclareSingleMethod, loc, display));
                return null;
            }

            var method = methods[0];
            if (!method.ReturnsVoid)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diags.MethodMustReturnVoid, loc, display, method.Name));
                return null;
            }

            var isNoData = type.ImplementsIEvent(s.IEvent);
            var constructedT = type.FindConstructedIEventT(s.IEventT);
            var isWithData = constructedT != null;

            if (isNoData && isWithData)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diags.CannotBeBothKinds, loc, display));
                return null;
            }

            if (!isNoData && !isWithData)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diags.MustPickOneKind, loc, display));
                return null;
            }

            var description = type.GetDescription(s.DescriptionAttr);
            var fqn = type.ToFqn();

            var pars = method.Parameters;
            if (isNoData)
            {
                if (pars.Length != 0)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(Diags.NoPayloadParamMismatch, loc, display, method.Name));
                    return null;
                }

                return new EventModel(fqn, display, method.Name, EventKind.NoPayload, null, description);
            }

            // isWithData
            if (pars.Length != 1)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diags.TooManyParameters, loc, display, method.Name));
                return null;
            }

            var declaredPayload = constructedT.TypeArguments[0];
            var methodPayload = pars[0].Type;
            if (!SymbolEqualityComparer.Default.Equals(declaredPayload, methodPayload))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Diags.PayloadParamMismatch, loc,
                    display, method.Name, declaredPayload.ToDisplayString(), methodPayload.ToDisplayString()));
                return null;
            }

            return new EventModel(fqn, display, method.Name, EventKind.SinglePayload,
                declaredPayload.ToFqn(), description);
        }
    }
}