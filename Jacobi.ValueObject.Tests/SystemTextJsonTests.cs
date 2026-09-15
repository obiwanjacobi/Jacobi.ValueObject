namespace Jacobi.ValueObject.Tests;

public class SystemTextJsonTests
{
    private readonly ITestOutputHelper _output;

    public SystemTextJsonTests(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void ValueObject_RoundTrips_AsScalarJson()
    {
        var decl = """
            [ValueObject<int>(ValueObjectOptions.SystemTextJson)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(42);
            var json = System.Text.Json.JsonSerializer.Serialize(vo);
            Assert.Equal("42", json);

            var copy = System.Text.Json.JsonSerializer.Deserialize<ValObj>(json);
            Assert.Equal(42, copy.Value);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void MultiValueObject_RoundTrips_WithCamelCasePropertyNames()
    {
        var decl = """
            [MultiValueObject(MultiValueObjectOptions.SystemTextJson)]
            public partial record struct MultiValObj
            {
                public partial int Id { get; }
                public partial string Name { get; }
            }
            """;
        var usage = """
            var vo = new MultiValObj(42, "name");
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var json = System.Text.Json.JsonSerializer.Serialize(vo, options);
            Assert.Equal("{\"id\":42,\"name\":\"name\"}", json);

            var copy = System.Text.Json.JsonSerializer.Deserialize<MultiValObj>(json, options);
            Assert.Equal(42, copy.Id);
            Assert.Equal("name", copy.Name);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void MultiValueObject_Deserializes_CaseInsensitive()
    {
        var decl = """
            [MultiValueObject(MultiValueObjectOptions.SystemTextJson)]
            public partial record struct MultiValObj
            {
                public partial int Id { get; }
                public partial string Name { get; }
            }
            """;
        var usage = """
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var json = "{\"ID\":42,\"NAME\":\"name\"}";
            var copy = System.Text.Json.JsonSerializer.Deserialize<MultiValObj>(json, options);
            Assert.Equal(42, copy.Id);
            Assert.Equal("name", copy.Name);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
