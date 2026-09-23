namespace Jacobi.ValueObject.Tests;

public class ValueObjectOperators
{
    private readonly ITestOutputHelper _output;

    public ValueObjectOperators(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void Op_Equals()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo1 = new ValObj(100);
            var vo2 = new ValObj(100);
            Assert.True(vo1 == vo2);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Op_NotEquals()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo1 = new ValObj(100);
            var vo2 = new ValObj(200);
            Assert.True(vo1 != vo2);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Op_GreaterThan()
    {
        var decl = """
            [ValueObject<int>(ValueObjectOptions.Comparable|ValueObjectOptions.Constructor)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo1 = new ValObj(100);
            var vo2 = new ValObj(200);
            Assert.True(vo2 > vo1);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Op_LesserThan()
    {
        var decl = """
            [ValueObject<int>(ValueObjectOptions.Comparable|ValueObjectOptions.Constructor)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo1 = new ValObj(100);
            var vo2 = new ValObj(200);
            Assert.True(vo1 < vo2);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
