namespace Jacobi.ValueObject.Tests;

public class ValueObjectTest
{
    private readonly ITestOutputHelper _output;

    public ValueObjectTest(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void RecordIEquatable()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(100);
            var intf = (IEquatable<ValObj>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void StructIEquatable()
    {
        var decl = """
            [ValueObject<int>]
            public partial struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(100);
            var intf = (IEquatable<ValObj>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void RecordToString()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(100);
            var str = vo.ToString();
            Assert.Equal("ValObj { Value = 100 }", str);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void StructToString()
    {
        var decl = """
            [ValueObject<int>]
            public partial struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(100);
            var str = vo.ToString();
            Assert.Equal("Test.ValObj", str);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Minimal_Int32_rec()
    {
        var decl = """
            [ValueObject<int>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(100);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Minimal_Int32()
    {
        var decl = """
            [ValueObject<int>]
            public partial struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(100);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Minimal_String_rec()
    {
        var decl = """
            [ValueObject<string>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj("Hello World");
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Minimal_String()
    {
        var decl = """
            [ValueObject<string>]
            public partial struct ValObj;
            """;
        var usage = """
            var vo = new ValObj("Hello World");
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Minimal_DateTime_rec()
    {
        var decl = """
            [ValueObject<DateTime>]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(DateTime.Now);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Minimal_DateTime()
    {
        var decl = """
            [ValueObject<DateTime>]
            public partial struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(DateTime.Now);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
