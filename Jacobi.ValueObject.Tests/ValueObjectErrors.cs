namespace Jacobi.ValueObject.Tests;

public class ValueObjectErrors
{
    [Fact]
    public void NoNamespace()
    {
        var source = """
            using Jacobi.ValueObject;
            [ValueObject<int>]
            public partial record struct ValObj;
            """;

        var diagnostics = Generator.Errors(source);
        Assert.Single(diagnostics);
        var error = diagnostics.First();
        Assert.Equal("VO001", error.Id);
    }

    [Fact]
    public void NoDatatype()
    {
        var source = """
            using Jacobi.ValueObject;
            namespace Test.Errors;
            [ValueObject(null)]
            public partial record struct ValObj;
            """;

        var diagnostics = Generator.Errors(source);
        Assert.Single(diagnostics);
        var error = diagnostics.First();
        Assert.Equal("VO002", error.Id);
    }

    [Fact]
    public void StringNotParsable()
    {
        var source = """
            using Jacobi.ValueObject;
            namespace Test.Errors;
            [ValueObject<string>(Options = ValueObjectOptions.Parsable)]
            public partial record struct ValObj;
            """;

        var diagnostics = Generator.Errors(source);
        Assert.Single(diagnostics);
        var error = diagnostics.First();
        Assert.Equal("VO003", error.Id);
    }
}
