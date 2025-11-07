namespace Jacobi.ValueObject.Tests;

public class ValueObjectMethods
{
    private readonly ITestOutputHelper _output;

    public ValueObjectMethods(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void DefaultConstructor_Error()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj();
            // should throw
            """;

        Generator.ExpectException<ValueObjectException>(decl, usage, _output);
    }

    [Fact]
    public void DefaultConstructorDecl_Error()
    {
        var source = """
            using Jacobi.ValueObject;
            namespace Test.Errors;
            [ValueObject<int>]
            public partial record struct ValObj();
            """;

        var diagnostics = Generator.Errors(source, _output);
        Assert.NotEmpty(diagnostics);
        Assert.Single(diagnostics, d => d.Id == "CS0111");
    }

    [Fact]
    public void Uninitialized_Error()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            var arr = new ValObj[1];
            var val = arr[0].Value;
            // should throw
            """;

        Generator.ExpectException<ValueObjectException>(decl, usage, _output);
    }

    [Fact]
    public void Static_IsValid()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj
            {
                public static bool IsValid(int value) => value == 42;
            }
            """;
        var usage = """
            var vo = new ValObj(42);
            // should not throw
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Static_IsValid_Try()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj
            {
                public static bool IsValid(int value) => value == 42;
            }
            """;
        var usage = """
            var valid = ValObj.Try(42, out var vo);
            Assert.True(valid);
            Assert.Equal(42, vo.Value);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Static_IsValid_Try_Invalid()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj
            {
                public static bool IsValid(int value) => value == 42;
            }
            """;
        var usage = """
            var valid = ValObj.Try(101, out var vo);
            Assert.False(valid);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Static_IsValid_Error()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj
            {
                public static bool IsValid(int value) => value == 42;
            }
            """;
        var usage = """
            var vo = new ValObj(101);
            // should throw
            """;

        Generator.ExpectException<ValueObjectException>(decl, usage, _output);
    }

    [Fact]
    public void Static_From()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj
            {
                public static partial ValObj From(int value);
            }
            """;
        var usage = """
            var vo = ValObj.From(42);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
