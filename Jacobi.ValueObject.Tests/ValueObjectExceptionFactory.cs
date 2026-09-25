namespace Jacobi.ValueObject.Tests;

public class ValueObjectExceptionFactory
{
    private readonly static string _exceptionFactory = """
        namespace Jacobi.ValueObject
        {
            internal static class ExceptionFactory
            {
                public static Exception NewConstructionException(string name)
                    => new System.InvalidOperationException($"You cannot call the default constructor for {name}.");

                public static Exception NewInitializationException(string name, string property)
                    => new System.ApplicationException($"Property '{property}' was not initialized on {name}.");

                public static Exception NewValidationException(string name, object value)
                    => new System.ArgumentException($"The value {value} is invalid for {name}.", "Value");

                public static Exception NewValidationException(string name, params IEnumerable<KeyValuePair<string, object>> properties)
                {
                    var props = String.Join(", ", properties.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    return new System.Exception($"Invalid {name} with {props}.");
                }
            }
        }
        """;

    private readonly ITestOutputHelper _output;

    public ValueObjectExceptionFactory(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void Single_Construction()
    {
        var decl = """
            #pragma warning disable CS0219  // warning for unused var (vo)
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            ValObj vo = default;    // should throw
            """;

        Generator.ExpectException<InvalidOperationException>(decl, usage, _exceptionFactory, _output);
    }

    [Fact]
    public void Single_Validation()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj
            {
                public static bool IsValid(int value) => value == 42;
            }
            """;
        var usage = """
            var vo = new ValObj(2112);
            // should throw
            """;

        Generator.ExpectException<ArgumentException>(decl, usage, _exceptionFactory, _output);
    }

    [Fact]
    public void Multi_Construction()
    {
        var decl = """
            #pragma warning disable CS0219  // warning for unused var (vo)
            [MultiValueObject]
            public partial record struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            MultiValObj vo = default;    // should throw
            """;

        Generator.ExpectException<InvalidOperationException>(decl, usage, _exceptionFactory, _output);
    }

    [Fact]
    public void Multi_Validation()
    {
        var decl = """
            #pragma warning disable CS0219  // warning for unused var (vo)
            [MultiValueObject]
            public partial record struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}

                public static bool IsValid(int id, string name) => id > 0 && !string.IsNullOrEmpty(name);
            }
            """;
        var usage = """
            var vo = new MultiValObj(42, "");    // should throw
            """;

        Generator.ExpectException<Exception>(decl, usage, _exceptionFactory, _output);
    }
}
