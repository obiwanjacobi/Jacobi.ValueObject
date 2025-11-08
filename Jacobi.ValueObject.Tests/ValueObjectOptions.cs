namespace Jacobi.ValueObject.Tests;

public class ValueObjectOptions
{
    private readonly ITestOutputHelper _output;

    public ValueObjectOptions(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void ImplicitFrom()
    {
        var decl = """
            [ValueObject<int>(Options = ValueObjectOptions.ImplicitFrom)]
            public partial record struct ValObj;
            """;
        var usage = """
            ValObj vo = 100;
            var intf = (IEquatable<int>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }


    [Fact]
    public void ImplicitAs()
    {
        var decl = """
            [ValueObject<int>(ValueObjectOptions.ImplicitAs | ValueObjectOptions.Constructor)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(42);
            int prim = vo;
            var intf = (IEquatable<int>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void ExplicitFrom()
    {
        var decl = """
            [ValueObject<int>(ValueObjectOptions.ExplicitFrom)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = ValObj.From(42);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void ToStringOption()
    {
        var decl = """
            [ValueObject<int>(ValueObjectOptions.ToString | ValueObjectOptions.Constructor)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(42);
            var str = vo.ToString();
            Assert.Equal("42", str);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Comparable()
    {
        var decl = """
            [ValueObject<int>(Options = ValueObjectOptions.Comparable | ValueObjectOptions.Constructor)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo1 = new ValObj(42);
            var vo2 = new ValObj(101);
            var greater = vo1 > vo2;
            Assert.False(greater);

            var comp = (IComparable<ValObj>)vo1;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void ComparableImplicit()
    {
        var decl = """
            [ValueObject<int>(Options = ValueObjectOptions.Comparable | ValueObjectOptions.ImplicitAs | ValueObjectOptions.Constructor)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(42);
            var greater = vo > 101;
            Assert.False(greater);

            var comp = (IComparable<ValObj>)vo;
            var compDatatype = (IComparable<int>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Parsable()
    {
        var decl = """
            [ValueObject<int>(Options = ValueObjectOptions.Parsable | ValueObjectOptions.Constructor)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = ValObj.Parse("42", null);
            Assert.Equal(42, vo.Value);

            var parse = (IParsable<ValObj>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
