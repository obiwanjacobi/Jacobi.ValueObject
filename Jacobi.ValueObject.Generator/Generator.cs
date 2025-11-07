using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jacobi.ValueObject.Generator;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var valObjInfos = FindDeclarationsAndSymbols(context);

        context.RegisterSourceOutput(valObjInfos, (spc, valObjInfo) =>
        {
            if (valObjInfo is null) return;
            if (valObjInfo.Symbol.ContainingNamespace.IsGlobalNamespace)
            {
                spc.NamespaceMissing(valObjInfo.Symbol.Name, valObjInfo.Declaration.GetLocation());
                return;
            }

            var valueObjectAttr = GetNameAndType(
                valObjInfo.Symbol, out var ns, out var name, out var datatype);

            if (datatype is null)
            {
                spc.DataTypeIsNull(valObjInfo.Symbol.Name, valObjInfo.Declaration.GetLocation());
                return;
            }

            var isValidMethod = FindMethod(valObjInfo.Declaration, "IsValid", "bool", datatype, isStatic: true, isPartial: false);
            var fromMethod = FindMethod(valObjInfo.Declaration, "From", name, datatype, isStatic: true, isPartial: true);

            ValueObjectOptions options = ValueObjectOptions.None;
            if (valueObjectAttr.NamedArguments.Length > 0)
            {
                var optionsArg = valueObjectAttr.NamedArguments[0];
                if (optionsArg.Value.Value is not null)
                    options = (ValueObjectOptions)optionsArg.Value.Value;
            }

            // default options - at least a constructor
            if (options == ValueObjectOptions.None)
                options = ValueObjectOptions.Constructor;

            var isRecordStruct = valObjInfo.Declaration.IsKind(SyntaxKind.RecordStructDeclaration);

            // determine what interfaces to implement.
            var interfaces = DetermineInterfaces(valObjInfo.Symbol.Interfaces, name, datatype);
            if (isRecordStruct) // record already implements this
                interfaces = interfaces & ~CodeBuilderInterfaces.IEquatableStruct;
            else  // add it for struct
                interfaces |= CodeBuilderInterfaces.IEquatableStruct;

            var hasImplicit = HasOption(options, ValueObjectOptions.ImplicitAs) | HasOption(options, ValueObjectOptions.ImplicitFrom);
            if (hasImplicit) interfaces |= CodeBuilderInterfaces.IEquatableValue;
            if (HasOption(options, ValueObjectOptions.Comparable))
            {
                interfaces |= CodeBuilderInterfaces.IComparableStruct;
                if (hasImplicit) interfaces |= CodeBuilderInterfaces.IComparableValue;
            }
            if (HasOption(options, ValueObjectOptions.Parsable))
                interfaces |= CodeBuilderInterfaces.IParsableStruct | CodeBuilderInterfaces.ISpanParsableStruct;

            if (HasInterface(interfaces, CodeBuilderInterfaces.IParsableStruct) &&
                (datatype == "System.String" || datatype == "string"))
            {
                spc.StringIsNotParsable(valObjInfo.Symbol.Name, valObjInfo.Declaration.GetLocation());
                return;
            }

            var builder = new CodeBuilder(interfaces)
                .Namespace(ns)
                .PartialStruct(name, datatype, isRecordStruct)
                .DefaultConstructor(name)
                .Constructor(name, datatype, HasOption(options, ValueObjectOptions.Constructor), isValidMethod is not null)
                .ValueProperty(name, datatype)
                ;

            if (HasOption(options, ValueObjectOptions.ImplicitFrom))
                builder.ImplicitFrom(name, datatype);
            if (HasOption(options, ValueObjectOptions.ImplicitAs))
                builder.ImplicitAs(name, datatype);
            if (HasOption(options, ValueObjectOptions.ExplicitFrom) || fromMethod is not null)
                builder.ExplicitFrom(name, datatype, isPartial: fromMethod is not null);
            if (HasOption(options, ValueObjectOptions.ToString))
                builder.ToString(name);
            if (isValidMethod is not null)
                builder.TryCreate(name, datatype);

            builder.AddInterfaceImplementations(name, datatype);

            spc.AddSource($"{name}_ValueObject.g.cs", builder.Build());
        });
    }

    private static MethodDeclarationSyntax? FindMethod(TypeDeclarationSyntax typeDecl, string name, string returnType, string parameterType, bool isStatic, bool isPartial)
    {
        var methods = typeDecl.Members.OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.Text == name);
        foreach (var method in methods)
        {

            if (isStatic == method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                isPartial == method.Modifiers.Any(SyntaxKind.PartialKeyword) &&
                method.ReturnType.ToString() == returnType)
            {
                return method;
            }
        }

        return null;
    }

    private static AttributeData GetNameAndType(INamedTypeSymbol symbol, out string ns, out string name, out string? datatype)
    {
        ns = symbol.ContainingNamespace.ToDisplayString();
        name = symbol.Name;

        // Find the ValueObjectAttribute on the symbol
        var valueObjectAttr = symbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "ValueObjectAttribute");
        if (valueObjectAttr is null || valueObjectAttr.AttributeClass?.Kind == SymbolKind.ErrorType)
            throw new InvalidOperationException("Internal Error: The ValueObjectAttribute is in Error!");

        datatype = null;
        if (valueObjectAttr.AttributeClass?.TypeArguments.Length > 0)
            // [ValueObject<T>]
            datatype = valueObjectAttr.AttributeClass?.TypeArguments[0].ToDisplayString();
        else if (valueObjectAttr.ConstructorArguments.Length > 0)
            // [ValueObject(typeof(T))]
            datatype = ((INamedTypeSymbol?)valueObjectAttr.ConstructorArguments[0].Value)?.ToDisplayString();

        return valueObjectAttr;
    }

    private static IncrementalValuesProvider<ValueObjectInfo?> FindDeclarationsAndSymbols(IncrementalGeneratorInitializationContext context)
    {
        var valObjInfos = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (node, _) =>
                            node is TypeDeclarationSyntax structDecl &&
                            structDecl.AttributeLists.Count > 0 &&
                            (structDecl.IsKind(SyntaxKind.StructDeclaration) || structDecl.IsKind(SyntaxKind.RecordStructDeclaration)),
                        transform: (ctx, _) =>
                        {
                            var model = ctx.SemanticModel;
                            var structDecl = (TypeDeclarationSyntax)ctx.Node;
                            var attributes = structDecl.AttributeLists.SelectMany(l => l.Attributes);
                            var valObj = attributes.FirstOrDefault(a =>
                            {
                                var name = a.Name.ToString();
                                return name == "ValueObject" || name.StartsWith("ValueObject<");
                            });
                            // is it our attribute?
                            if (valObj != null)
                            {
                                var symbol = model.GetDeclaredSymbol(structDecl);
                                if (symbol is not null)
                                {
                                    return new ValueObjectInfo(structDecl, symbol);
                                }
                            }
                            return null;
                        })
                    .Where(valObjInfo => valObjInfo is not null);
        return valObjInfos;
    }

    private CodeBuilderInterfaces DetermineInterfaces(ImmutableArray<INamedTypeSymbol> interfaces, string name, string datatype)
    {
        var interfaceOptions = CodeBuilderInterfaces.None;

        foreach (var intf in interfaces)
        {
            if (intf.MetadataName == "IComparable`1")
            {
                if (intf.TypeArguments[0].Name == name)
                    interfaceOptions |= CodeBuilderInterfaces.IComparableStruct;
                if (intf.TypeArguments[0].ToDisplayString() == datatype)
                    interfaceOptions |= CodeBuilderInterfaces.IComparableValue;
            }

            if (intf.MetadataName == "IEquatable`1")
            {
                if (intf.TypeArguments[0].Name == name)
                    interfaceOptions |= CodeBuilderInterfaces.IEquatableStruct;
                if (intf.TypeArguments[0].ToDisplayString() == datatype)
                    interfaceOptions |= CodeBuilderInterfaces.IEquatableValue;
            }

            if (intf.MetadataName == "IParsable`1")
            {
                if (intf.TypeArguments[0].Name == name)
                    interfaceOptions |= CodeBuilderInterfaces.IParsableStruct;
            }
            if (intf.MetadataName == "ISpanParsable`1")
            {
                if (intf.TypeArguments[0].Name == name)
                    interfaceOptions |= CodeBuilderInterfaces.ISpanParsableStruct | CodeBuilderInterfaces.IParsableStruct;
            }
        }

        return interfaceOptions;
    }

    private static bool HasOption(ValueObjectOptions options, ValueObjectOptions testForOption)
        => (options & testForOption) == testForOption;

    private static bool HasInterface(CodeBuilderInterfaces interfaces, CodeBuilderInterfaces intf)
        => (interfaces & intf) == intf;

    private record class ValueObjectInfo(TypeDeclarationSyntax Declaration, INamedTypeSymbol Symbol);
}
