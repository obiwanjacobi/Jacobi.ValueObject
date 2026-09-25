using Microsoft.CodeAnalysis;

namespace Jacobi.ValueObject.Generator;

internal static class Extensions
{
    public static string LowerFirstChar(this string input)
        => new([Char.ToLowerInvariant(input[0]), .. input.Substring(1)]);


    public static bool HasExceptionFactory(this Compilation compilation)
    {
        var exceptionFactory = compilation.GetTypeByMetadataName("Jacobi.ValueObject.ExceptionFactory");
        return exceptionFactory is not null
            && exceptionFactory.TypeKind == TypeKind.Class
            && exceptionFactory.IsStatic
            && exceptionFactory.ContainingNamespace.ToDisplayString() == "Jacobi.ValueObject";
    }
}
