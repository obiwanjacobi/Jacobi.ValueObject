namespace Jacobi.ValueObject.Generator;

internal static class Extensions
{
    public static string LowerFirstChar(this string input)
        => new([Char.ToLowerInvariant(input[0]), .. input.Substring(1)]);
}

