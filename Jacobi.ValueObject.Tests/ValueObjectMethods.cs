namespace Jacobi.ValueObject.Tests;

public class ValueObjectMethods
{
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

        Generator.ExpectException<ValueObjectException>(decl, usage);
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

        Generator.ExpectException<ValueObjectException>(decl, usage);
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

        Generator.AssertAndRun(decl, usage);
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

        Generator.ExpectException<ValueObjectException>(decl, usage);
    }
}
