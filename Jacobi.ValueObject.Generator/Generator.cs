using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jacobi.ValueObject.Generator;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var valObjInfos = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) =>
                    node is RecordDeclarationSyntax structDecl &&
                    structDecl.AttributeLists.Count > 0 &&
                    structDecl.Kind() == SyntaxKind.RecordStructDeclaration,
                transform: (ctx, _) =>
                {
                    var model = ctx.SemanticModel;
                    var structDecl = (RecordDeclarationSyntax)ctx.Node;
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

        context.RegisterSourceOutput(valObjInfos, (spc, valObjInfo) =>
        {
            if (valObjInfo is null) return;
            if (valObjInfo.Symbol.ContainingNamespace.IsGlobalNamespace)
            {
                spc.NamespaceMissing(valObjInfo.Symbol.Name, valObjInfo.Declaration.GetLocation());
                return;
            }

            var ns = valObjInfo.Symbol.ContainingNamespace.ToDisplayString();
            var name = valObjInfo.Symbol.Name;

            // Find the ValueObjectAttribute on the symbol
            var valueObjectAttr = valObjInfo.Symbol.GetAttributes()
                .FirstOrDefault(attr =>
                {
#if DEBUG
                    // For tests the AttributeClass.Name is 'ValueObject' in real code it's 'ValueObjectAttribute'!?
                    return attr.AttributeClass?.Name == "ValueObjectAttribute" || attr.AttributeClass?.Name == "ValueObject";
#else
                    return attr.AttributeClass?.Name == "ValueObjectAttribute";
#endif // DEBUG
                });

            if (valueObjectAttr is null) return;
            if (valueObjectAttr.AttributeClass?.Kind == SymbolKind.ErrorType)
                throw new InvalidOperationException("Internal Error: The ValueObjectAttribute is in Error!");

            string? datatype = null;
            if (valueObjectAttr.AttributeClass?.TypeArguments.Length > 0)
                // [ValueObject<T>]
                datatype = valueObjectAttr.AttributeClass?.TypeArguments[0].ToDisplayString();
            else if (valueObjectAttr.ConstructorArguments.Length > 0)
                // [ValueObject(typeof(T))]
                datatype = ((INamedTypeSymbol?)valueObjectAttr.ConstructorArguments[0].Value)?.ToDisplayString();

            if (datatype is null)
            {
                spc.DataTypeIsNull(valObjInfo.Symbol.Name, valObjInfo.Declaration.GetLocation());
                return;
            }

            var isValidMethod = valObjInfo.Declaration.Members.OfType<MethodDeclarationSyntax>()
                .SingleOrDefault(m => m.Modifiers.Any(SyntaxKind.StaticKeyword) && m.Identifier.Text == "IsValid");

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

            if (HasOption(options, ValueObjectOptions.Parsable) && (datatype == "System.String" || datatype == "string"))
            {
                spc.StringIsNotParsable(valObjInfo.Symbol.Name, valObjInfo.Declaration.GetLocation());
                return;
            }

            // determine what interfaces to implement.
            var interfaces = CodeBuilderInterfaces.None;
            var hasImplicit = HasOption(options, ValueObjectOptions.ImplicitAs) | HasOption(options, ValueObjectOptions.ImplicitFrom);
            if (hasImplicit) interfaces |= CodeBuilderInterfaces.IEquatableValue;
            if (HasOption(options, ValueObjectOptions.Comparable))
            {
                interfaces |= CodeBuilderInterfaces.IComparableStruct;
                if (hasImplicit) interfaces |= CodeBuilderInterfaces.IComparableValue;
            }
            if (HasOption(options, ValueObjectOptions.Parsable))
                interfaces |= CodeBuilderInterfaces.IParsableStruct;

            var builder = new CodeBuilder(interfaces)
                .Namespace(ns)
                .PartialRecordStruct(name, datatype)
                .DefaultConstructor(name)
                .Constructor(name, datatype, HasOption(options, ValueObjectOptions.Constructor), isValidMethod)
                .ValueProperty(name, datatype)
                ;

            if (HasOption(options, ValueObjectOptions.ImplicitFrom))
                builder.ImplicitFrom(name, datatype);
            if (HasOption(options, ValueObjectOptions.ImplicitAs))
                builder.ImplicitAs(name, datatype);
            if (HasOption(options, ValueObjectOptions.ExplicitFrom))
                builder.ExplicitFrom(name, datatype);
            if (HasOption(options, ValueObjectOptions.ToString))
                builder.ToString(name);

            builder.AddInterfaceImplementations(name, datatype);

            spc.AddSource($"{name}_ValueObject.g.cs", builder.Build());
        });
    }

    private static bool HasOption(ValueObjectOptions options, ValueObjectOptions testForOption)
        => (options & testForOption) == testForOption;

    private record class ValueObjectInfo(RecordDeclarationSyntax Declaration, INamedTypeSymbol Symbol);
}
