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
            [MultiValueObject(MultiValueObjectOptions.ExplicitFrom)]
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

    [Fact]
    public void Deconstruct()
    {
        var decl = """
            [MultiValueObject(MultiValueObjectOptions.Deconstruct | MultiValueObjectOptions.Constructor)]
            public partial struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            var vo = new MultiValObj(100, "name");
            (var id, var name) = vo;
            Assert.Equal(100, id);
            Assert.Equal("name", name);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void Deconstruct_rec()
    {
        var decl = """
            [MultiValueObject(MultiValueObjectOptions.Deconstruct | MultiValueObjectOptions.Constructor)]
            public partial record struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            var vo = new MultiValObj(100, "name");
            (var id, var name) = vo;
            Assert.Equal(100, id);
            Assert.Equal("name", name);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void ValueObjectNestedInMulti()
    {
        var decl = """
            [ValueObject<int>(Options = ValueObjectOptions.ImplicitFrom | ValueObjectOptions.Constructor)]
            public partial struct ProductId;

            [MultiValueObject]
            public partial record struct MultiValObj
            {
                public partial ProductId Id {get;}
                public partial string Name {get;}
            }
            """;
        var usage = """
            var vo = new MultiValObj(100, "name");
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void WithKeyword_Error()
    {
        var source = """
            using Jacobi.ValueObject;
            namespace Test.Errors;
            [MultiValueObject]
            public partial record struct MultiValObj
            {
                public partial int Id {get;}
                public partial string Name {get;}
            }
            public static class Program {
                public static void Main() {
                    var vo1 = new MultiValObj(100, "name");
                    var vo2 = vo1 with { Id = 42 };
            }}
            """;

        var diagnostics = Generator.Errors(source, _output);
        Assert.NotEmpty(diagnostics);
        var error = diagnostics.First();
        Assert.Equal("CS0200", error.Id);
    }

    [Fact]
    public void MultiPropertiesInit_Error()
    {
        var source = """
            using Jacobi.ValueObject;
            namespace Test.Errors;
            [MultiValueObject]
            public partial record struct MultiValObj
            {
                public partial int Id {get;init;}
                public partial string Name {get;}
            }
            public static class Program {
                public static void Main() {
                    var vo = new MultiValObj(100, "name");
            }}
            """;

        var diagnostics = Generator.Errors(source, _output);
        Assert.NotEmpty(diagnostics);
        var error = diagnostics.First();
        Assert.Equal("VO004", error.Id);
    }

    [Fact]
    public void MultiPropertiesRef_Error()
    {
        var source = """
            using Jacobi.ValueObject;
            namespace Test.Errors;
            [MultiValueObject]
            public partial record struct MultiValObj
            {
                public partial ref int Id {get;}
                public partial string Name {get;}
            }
            public static class Program {
                public static void Main() {
                    var vo = new MultiValObj(100, "name");
            }}
            """;

        var diagnostics = Generator.Errors(source, _output);
        Assert.NotEmpty(diagnostics);
        var error = diagnostics.First();
        Assert.Equal("VO005", error.Id);
    }
}
