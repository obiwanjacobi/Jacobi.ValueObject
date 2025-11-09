using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jacobi.ValueObject.Generator;

[Generator]
public sealed class MultiGenerator : IIncrementalGenerator
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

            var ns = valObjInfo.Symbol.ContainingNamespace.ToDisplayString();
            var name = valObjInfo.Symbol.Name;

            var valueObjectAttr = valObjInfo.Symbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name == "MultiValueObjectAttribute");
            if (valueObjectAttr is null || valueObjectAttr.AttributeClass?.Kind == SymbolKind.ErrorType)
                throw new InvalidOperationException("Internal Error: The ValueObjectAttribute is in Error!");

            MultiValueObjectOptions options = MultiValueObjectOptions.None;
            if (valueObjectAttr.ConstructorArguments.Length > 0)
            {
                var optionsArg = valueObjectAttr.ConstructorArguments[0];
                if (optionsArg.Value is not null)
                    options = (MultiValueObjectOptions)optionsArg.Value;
            }

            var properties = FindProperties(valObjInfo.Declaration, valObjInfo.Model);
            var isValidMethod = FindMethod(valObjInfo.Declaration, "IsValid", "bool", [.. properties.Select(p => p.Value.type)], isStatic: true, isPartial: false);
            var fromMethod = FindMethod(valObjInfo.Declaration, "From", name, [.. properties.Select(p => p.Value.type)], isStatic: true, isPartial: true);

            // default options - at least a constructor
            if (options == MultiValueObjectOptions.None)
                options = MultiValueObjectOptions.Constructor;

            var isRecordStruct = valObjInfo.Declaration.IsKind(SyntaxKind.RecordStructDeclaration);

            // determine what interfaces to implement.
            var interfaces = CodeBuilderInterfaces.None;
            if (isRecordStruct) // record already implements this
                interfaces = interfaces & ~CodeBuilderInterfaces.IEquatableStruct;
            else  // add it for struct
                interfaces |= CodeBuilderInterfaces.IEquatableStruct;

            var builder = new CodeBuilder(interfaces)
                .Namespace(ns)
                .PartialStruct(name, null, isRecordStruct, isMulti: true)
                .DefaultConstructor(name)
                .Constructor(name, properties, HasOption(options, MultiValueObjectOptions.Constructor), isValidMethod is not null)
                .Properties(properties, name)
                ;

            if (!isRecordStruct)
            {
                builder.ObjectToString(properties, name);
                builder.OverrideEqualsAndGetHashCode(properties, name);
            }
            if (HasOption(options, MultiValueObjectOptions.ExplicitFrom) || fromMethod is not null)
                builder.ExplicitFrom(name, properties, isPartial: fromMethod is not null);
            if (HasOption(options, MultiValueObjectOptions.Deconstruct))
                builder.Deconstruct(properties);
            if (isValidMethod is not null)
                builder.TryCreate(properties, name);

            builder.AddInterfaceImplementations(properties, name);

            spc.AddSource($"{name}_ValueObject.g.cs", builder.Build());
        });
    }

    private static IDictionary<string, (string type, bool isStruct)> FindProperties(TypeDeclarationSyntax typeDecl, SemanticModel model)
    {
        var properties = typeDecl.Members.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(SyntaxKind.PartialKeyword) && p.Modifiers.Any(SyntaxKind.PublicKeyword));

        var props = properties.ToDictionary(p => p.Identifier.Text, p =>
            {
                var type = model.GetDeclaredSymbol(p)!.Type;
                return (type: p.Type.ToString(), isStruct: type.TypeKind == TypeKind.Struct);
            });

        return props!;
    }

    private static MethodDeclarationSyntax? FindMethod(TypeDeclarationSyntax typeDecl, string name, string returnType, string[] parameterTypes, bool isStatic, bool isPartial)
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
                    var valObj = attributes.FirstOrDefault(a => a.Name.ToString() == "MultiValueObject");
                    // is it our attribute?
                    if (valObj is not null)
                    {
                        var symbol = model.GetDeclaredSymbol(structDecl);
                        if (symbol is not null)
                        {
                            return new ValueObjectInfo(structDecl, symbol, model);
                        }
                    }
                    return null;
                })
            .Where(valObjInfo => valObjInfo is not null);
        return valObjInfos;
    }

    private static bool HasOption(MultiValueObjectOptions options, MultiValueObjectOptions testForOption)
        => (options & testForOption) == testForOption;
}
