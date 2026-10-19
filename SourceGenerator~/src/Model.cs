using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace AceLand.EventDriven.SourceGenerator
{
    /// <summary>Well-known symbols resolved once per compilation via metadata name.</summary>
    internal sealed class Symbols
    {
        public const string IBusEventMetadataName = "AceLand.EventDriven.Bus.IBusEvent";
        public const string IEventMetadataName = "AceLand.EventDriven.Bus.IEvent";
        public const string IEventTMetadataName = "AceLand.EventDriven.Bus.IEvent`1";
        public const string DescriptionAttrMetadataName = "AceLand.EventDriven.Bus.EventDescriptionAttribute";

        private Symbols(INamedTypeSymbol iBusEvent, INamedTypeSymbol iEvent,
            INamedTypeSymbol iEventT, INamedTypeSymbol descriptionAttr)
        {
            IBusEvent = iBusEvent;
            IEvent = iEvent;
            IEventT = iEventT;
            DescriptionAttr = descriptionAttr;
        }

        public INamedTypeSymbol IBusEvent { get; }
        public INamedTypeSymbol IEvent { get; }
        public INamedTypeSymbol IEventT { get; }
        public INamedTypeSymbol DescriptionAttr { get; }

        /// <summary>Returns null (caller must early-return) when IBusEvent is not referenced.</summary>
        public static Symbols TryCreate(Compilation c)
        {
            var iBusEvent = c.GetTypeByMetadataName(IBusEventMetadataName);
            if (iBusEvent is null) return null;

            var iEvent = c.GetTypeByMetadataName(IEventMetadataName);
            var iEventT = c.GetTypeByMetadataName(IEventTMetadataName);
            if (iEvent is null || iEventT is null) return null;

            // Description attribute is optional; may be null when not referenced.
            var descriptionAttr = c.GetTypeByMetadataName(DescriptionAttrMetadataName);

            return new Symbols(iBusEvent, iEvent, iEventT, descriptionAttr);
        }
    }

    internal enum EventKind
    {
        NoPayload,
        SinglePayload,
    }

    /// <summary>Validated metadata for a single event interface, ready for emission.</summary>
    internal sealed class EventModel
    {
        public EventModel(string fullyQualifiedName, string displayName, string methodName,
            EventKind kind, string payloadFqn, string description)
        {
            FullyQualifiedName = fullyQualifiedName;
            DisplayName = displayName;
            MethodName = methodName;
            Kind = kind;
            PayloadFqn = payloadFqn;
            Description = description;
        }

        /// <summary>global::-qualified name, used in generated code.</summary>
        public string FullyQualifiedName { get; }

        /// <summary>Human-readable name for registry keys / diagnostics.</summary>
        public string DisplayName { get; }

        public string MethodName { get; }
        public EventKind Kind { get; }

        /// <summary>global::-qualified payload type; null when <see cref="Kind"/> is NoPayload.</summary>
        public string PayloadFqn { get; }

        /// <summary>From [EventDescription]; null when absent.</summary>
        public string Description { get; }
    }

    internal static class SymbolExtensions
    {
        private static readonly SymbolDisplayFormat FullyQualified =
            SymbolDisplayFormat.FullyQualifiedFormat;

        public static string ToFqn(this ITypeSymbol symbol)
            => symbol.ToDisplayString(FullyQualified);

        public static bool ImplementsDirectlyOrIndirectly(this INamedTypeSymbol type, INamedTypeSymbol target)
        {
            foreach (var i in type.AllInterfaces)
                if (SymbolEqualityComparer.Default.Equals(i, target))
                    return true;
            return false;
        }

        /// <summary>Returns the constructed IEvent&lt;T&gt; interface, or null when absent.</summary>
        public static INamedTypeSymbol FindConstructedIEventT(this INamedTypeSymbol type, INamedTypeSymbol openIEventT)
        {
            foreach (var i in type.AllInterfaces)
            {
                if (i.IsGenericType &&
                    SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, openIEventT))
                    return i;
            }
            return null;
        }

        public static bool ImplementsIEvent(this INamedTypeSymbol type, INamedTypeSymbol iEvent)
        {
            foreach (var i in type.AllInterfaces)
                if (SymbolEqualityComparer.Default.Equals(i, iEvent))
                    return true;
            return false;
        }

        public static string GetDescription(this INamedTypeSymbol type, INamedTypeSymbol descriptionAttr)
        {
            if (descriptionAttr is null) return null;

            foreach (var attr in type.GetAttributes())
            {
                if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, descriptionAttr))
                    continue;
                if (attr.ConstructorArguments.Length > 0 &&
                    attr.ConstructorArguments[0].Value is string s)
                    return s;
            }
            return null;
        }

        public static IEnumerable<IMethodSymbol> OrdinaryMethods(this INamedTypeSymbol type)
        {
            foreach (var m in type.GetMembers())
                if (m is IMethodSymbol { MethodKind: MethodKind.Ordinary } method)
                    yield return method;
        }
    }
}