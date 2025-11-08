namespace Jacobi.ValueObject.Tests;

public class MultiValueObjectTest
{
    private readonly ITestOutputHelper _output;

    public MultiValueObjectTest(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void RecordIEquatable()
    {
        var decl = """
            [MultiValueObject]
            public partial record struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            var vo = new MultiValObj(100, "name");
            var intf = (IEquatable<MultiValObj>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void StructIEquatable()
    {
        var decl = """
            [MultiValueObject]
            public partial struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            var vo = new MultiValObj(100, "name");
            var intf = (IEquatable<MultiValObj>)vo;
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void StructEquals()
    {
        var decl = """
            [MultiValueObject]
            public partial struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            var vo1 = new MultiValObj(42, "name");
            var vo2 = new MultiValObj(100, "name");
            var equal = vo1 == vo2;
            Assert.False(equal);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void ExplicitFrom()
    {
        var decl = """
            [MultiValueObject(Options = MultiValueObjectOptions.ExplicitFrom)]
            public partial struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            var vo = MultiValObj.From(100, "name");
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void IsValid_Try()
    {
        var decl = """
            [MultiValueObject]
            public partial struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}

                public static bool IsValid(int id, string name)
                    => true;
            }
            """;
        var usage = """
            var valid = MultiValObj.Try(100, "name", out var vo);
            Assert.True(valid);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
