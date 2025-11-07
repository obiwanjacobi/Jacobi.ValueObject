namespace Jacobi.ValueObject.Tests;

public class ValueObjectInterfaces
{
    private readonly ITestOutputHelper _output;

    public ValueObjectInterfaces(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void ComparableStruct()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj : IComparable<ValObj>;
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
    public void ComparableValue()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj : IComparable<int>;
            """;
        var usage = """
            var vo = new ValObj(42);
            var greater = vo > 101;
            Assert.False(greater);

            var comp = (IComparable<int>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void EquatableValue()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj : IEquatable<int>;
            """;
        var usage = """
            ValObj vo = new ValObj(42);
            var equal = vo == 100;
            Assert.False(equal);
            var intf = (IEquatable<int>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void ParsableStruct()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj : IParsable<ValObj>;
            """;
        var usage = """
            var vo = ValObj.Parse("42", null);
            Assert.Equal(42, vo.Value);

            var parse = (IParsable<ValObj>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void SpanParsableStruct()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj : ISpanParsable<ValObj>;
            """;
        var usage = """
            var vo = ValObj.Parse("42".AsSpan(), null);
            Assert.Equal(42, vo.Value);

            var parse = (ISpanParsable<ValObj>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
