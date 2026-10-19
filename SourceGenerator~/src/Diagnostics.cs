using Microsoft.CodeAnalysis;

namespace AceLand.EventDriven.SourceGenerator
{
    internal static class Diags
    {
        const string Cat = "AceLand.EventDriven";

        public static readonly DiagnosticDescriptor GeneratorCrashed = new DiagnosticDescriptor(
            "ACEEV000", "EventDriven generator crashed",
            "AceLand.EventDriven.SourceGenerator threw: {0}",
            Cat, DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor MustDeclareSingleMethod = new DiagnosticDescriptor(
            "ACEEV001", "Event interface must declare exactly one method",
            "Event interface '{0}' must declare exactly one method", Cat, DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor MethodMustReturnVoid = new DiagnosticDescriptor(
            "ACEEV002", "Event method must return void",
            "Event method '{0}.{1}' must return void", Cat, DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor CannotBeBothKinds = new DiagnosticDescriptor(
            "ACEEV003", "Event cannot implement both IEvent and IEvent<T>",
            "Event interface '{0}' cannot implement both IEvent and IEvent<T>", Cat, DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor MustPickOneKind = new DiagnosticDescriptor(
            "ACEEV004", "Event must implement IEvent or IEvent<T>",
            "Event interface '{0}' must implement either IEvent or IEvent<T>", Cat, DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor NoPayloadParamMismatch = new DiagnosticDescriptor(
            "ACEEV005", "IEvent method must have 0 parameters",
            "Event method '{0}.{1}' implements IEvent but does not declare 0 parameters", Cat, DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor PayloadParamMismatch = new DiagnosticDescriptor(
            "ACEEV006", "IEvent<T> method must have 1 matching parameter",
            "Event method '{0}.{1}' implements IEvent<{2}> but its single parameter is '{3}'; the payload types must match exactly",
            Cat, DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor TooManyParameters = new DiagnosticDescriptor(
            "ACEEV007", "Event method must have 0 or 1 parameters",
            "Event method '{0}.{1}' must declare 0 or 1 parameters", Cat, DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor OpenGenericUnsupported = new DiagnosticDescriptor(
            "ACEEV008", "Open generic event interfaces are not supported",
            "Event interface '{0}' is an open generic type; event interfaces must be non-generic", Cat, DiagnosticSeverity.Error, true);
    }
}