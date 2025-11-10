using Microsoft.CodeAnalysis;

namespace Jacobi.ValueObject.Generator;

internal static class DiagnosticExtensions
{
    public static void NamespaceMissing(this SourceProductionContext context, string name, Location location)
        => context.ReportDiagnostic(Diagnostic.Create(MissingNamespaceDescriptor, location, name));

    public static void DataTypeIsNull(this SourceProductionContext context, string name, Location location)
        => context.ReportDiagnostic(Diagnostic.Create(DataTypeIsNullDescriptor, location, name));

    public static void StringIsNotParsable(this SourceProductionContext context, string name, Location location)
        => context.ReportDiagnostic(Diagnostic.Create(StringIsNotParsableDescriptor, location, name));
    public static void PropertyMustBeReadOnly(this SourceProductionContext context, string name, string property, Location location)
        => context.ReportDiagnostic(Diagnostic.Create(PropertyMustBeReadOnlyDescriptor, location, name, property));
    public static void PropertyNotReturnByRef(this SourceProductionContext context, string name, string property, Location location)
        => context.ReportDiagnostic(Diagnostic.Create(PropertyNotReturnByRefDescriptor, location, name, property));

    private static readonly DiagnosticDescriptor MissingNamespaceDescriptor = new(
        id: "VO001",
        title: "Value Object must be declared inside a namespace",
        messageFormat: "The value object '{0}' must not be declared in the global namespace",
        category: "ValueObject",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DataTypeIsNullDescriptor = new(
        id: "VO002",
        title: "Value Object must have a valid data type",
        messageFormat: "The value object '{0}' does not seem to have a data type: [ValueObject(typeof(data-type-here))] or [ValueObject<data-type-here>]",
        category: "ValueObject",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor StringIsNotParsableDescriptor = new(
        id: "VO003",
        title: "Cannot specify the Parsable option for type string (System.String)",
        messageFormat: "The value object '{0}' specified the Parsable option which is not compatible with the System.String type",
        category: "ValueObject",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PropertyMustBeReadOnlyDescriptor = new(
        id: "VO004",
        title: "The property declaration on a multi-value object must be read-only",
        messageFormat: "The property '{1}' on value object '{0}' specified a 'set' or 'init' accessor. Properties must only specify the 'get' accessor.",
        category: "ValueObject",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PropertyNotReturnByRefDescriptor = new(
        id: "VO005",
        title: "The property declaration on a multi-value object must not return by ref",
        messageFormat: "The property '{1}' on value object '{0}' specified a 'ref' in the return type. Properties must only specify the type.",
        category: "ValueObject",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

