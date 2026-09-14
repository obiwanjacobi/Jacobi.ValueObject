namespace Jacobi.ValueObject.Tests;

public class NewtonsoftJsonTests
{
    private readonly ITestOutputHelper _output;

    public NewtonsoftJsonTests(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void ValueObject_RoundTrips_AsScalarJson()
    {
        var decl = """
            [ValueObject<int>(ValueObjectOptions.NewtonsoftJson)]
            public partial record struct ValObj;
            """;
        var usage = """
            var vo = new ValObj(42);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(vo);
            Assert.Equal("42", json);

            var copy = Newtonsoft.Json.JsonConvert.DeserializeObject<ValObj>(json);
            Assert.Equal(42, copy.Value);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void MultiValueObject_RoundTrips_WithCamelCasePropertyNames()
    {
        var decl = """
            [MultiValueObject(MultiValueObjectOptions.NewtonsoftJson)]
            public partial record struct MultiValObj
            {
                public partial int Id { get; }
                public partial string Name { get; }
            }
            """;
        var usage = """
            var vo = new MultiValObj(42, "name");
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(vo, settings);
            Assert.Equal("{\"id\":42,\"name\":\"name\"}", json);

            var copy = Newtonsoft.Json.JsonConvert.DeserializeObject<MultiValObj>(json, settings);
            Assert.Equal(42, copy.Id);
            Assert.Equal("name", copy.Name);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }

    [Fact]
    public void MultiValueObject_Deserializes_CaseInsensitive()
    {
        var decl = """
            [MultiValueObject(MultiValueObjectOptions.NewtonsoftJson)]
            public partial record struct MultiValObj
            {
                public partial int Id { get; }
                public partial string Name { get; }
            }
            """;
        var usage = """
            var settings = new Newtonsoft.Json.JsonSerializerSettings();

            var json = "{\"ID\":42,\"NAME\":\"name\"}";
            var copy = Newtonsoft.Json.JsonConvert.DeserializeObject<MultiValObj>(json, settings);
            Assert.Equal(42, copy.Id);
            Assert.Equal("name", copy.Name);
            """;

        Generator.AssertAndRun(decl, usage, _output);
    }
}
