using System.Reflection;
using System.Text;

namespace Jacobi.ValueObject.Generator;

using IProperties = IDictionary<string, (string type, bool isStruct)>;

internal sealed class CodeBuilder
{
    private readonly StringBuilder _builder = new();
    private readonly Stack<string> _dedents = new();
    private int _indent = 0;

    private readonly CodeBuilderInterfaces _interfaces;
    public CodeBuilder(CodeBuilderInterfaces interfaces)
    {
        _interfaces = interfaces;
    }

    public CodeBuilder Namespace(string name)
    {
        Indent().AppendLine("#nullable enable");
        Indent().AppendLine("#pragma warning disable CS8604 // Possible null reference argument for parameter...");
        Indent().AppendLine($"namespace {name}");
        Scope();
        return this;
    }

    public CodeBuilder PartialStruct(string name, string? datatype, bool isRecord, bool isMulti)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        Indent().AppendLine($"""[System.CodeDom.Compiler.GeneratedCode("{assemblyName.Name}", "{assemblyName.Version}")]""");
        if (!isMulti) Indent().AppendLine($$"""[System.Diagnostics.DebuggerDisplay("Value = {Value}")]""");
        Indent().Append("readonly partial ")
            .Append(isRecord ? "record " : "")
            .Append($"struct {name}");
        if (_interfaces != CodeBuilderInterfaces.None)
        {
            var addComma = false;
            _builder.Append(" : ");
            if ((_interfaces & CodeBuilderInterfaces.IEquatableStruct) != 0)
            {
                if (addComma) _builder.Append(", ");
                _builder.Append($"System.IEquatable<{name}> ");
                addComma = true;
            }
            if ((_interfaces & CodeBuilderInterfaces.IEquatableValue) != 0)
            {
                if (addComma) _builder.Append(", ");
                _builder.Append($"System.IEquatable<{datatype}> ");
                addComma = true;
            }
            if ((_interfaces & CodeBuilderInterfaces.IComparableStruct) != 0)
            {
                if (addComma) _builder.Append(", ");
                _builder.Append($"System.IComparable<{name}>");
                addComma = true;
            }
            if ((_interfaces & CodeBuilderInterfaces.IComparableValue) != 0)
            {
                if (addComma) _builder.Append(", ");
                _builder.Append($"System.IComparable<{datatype}>");
                addComma = true;
            }
            if ((_interfaces & CodeBuilderInterfaces.IParsableStruct) != 0)
            {
                if (addComma) _builder.Append(", ");
                _builder.Append($"System.IParsable<{name}>");
                addComma = true;
            }
            if ((_interfaces & CodeBuilderInterfaces.ISpanParsableStruct) != 0)
            {
                if (addComma) _builder.Append(", ");
                _builder.Append($"System.ISpanParsable<{name}>");
                addComma = true;
            }

            _builder.AppendLine();
        }
        Scope();
        return this;
    }

    public CodeBuilder DefaultConstructor(string name)
    {
        Indent().AppendLine($"""public {name}() => throw new Jacobi.ValueObject.ValueObjectException("Do not call the default constructor for ValueObject '{name}'.");""");
        return this;
    }

    public CodeBuilder Constructor(string name, string datatype, bool isPublic, bool hasIsValidMethod)
    {
        Indent()
            .Append(isPublic ? "public" : "private")
            .Append($" {name}({datatype} value) ")
            .AppendLine(!hasIsValidMethod ? "=> _value = value;"
                : $$"""{ if ({{name}}.IsValid(value)) _value = value; else throw new Jacobi.ValueObject.ValueObjectException($"Validation Failed. The value '{value}' is not valid for Value Object '{{name}}'."); }""");
        return this;
    }

    public CodeBuilder Constructor(string name, IProperties properties, bool isPublic, bool hasIsValidMethod)
    {
        Indent()
                    .Append(isPublic ? "public" : "private")
                    .Append($" {name}(")
                    .AppendPropertiesAsParameters(properties)
                    .AppendLine(")");

        Scope();
        if (hasIsValidMethod)
        {

            Indent().Append($"if ({name}.IsValid(")
                .AppendPropertiesAsArguments(properties, asMembers: false)
                .AppendLine("))");
            Scope();
            Indent().AppendPropertiesAssignments(properties, toPrivates: true).AppendLine();
            EndScope();
            Indent().Append("else ")
                .AppendLine($$"""throw new Jacobi.ValueObject.ValueObjectException($"Validation Failed. The specified values are not valid for Value Object '{{name}}'.");""");
        }
        else
        {
            Indent().AppendPropertiesAssignments(properties, toPrivates: true).AppendLine();
        }
        EndScope();
        return this;
    }

    public CodeBuilder ValueProperty(string name, string datatype)
    {
        Indent().AppendLine($"private readonly {datatype}? _value;");
        Indent().AppendLine($"""public {datatype} Value => _value ?? throw new Jacobi.ValueObject.ValueObjectException("ValueObject '{name}' was not initialized with a valid value.");""");
        return this;
    }

    public CodeBuilder Properties(IProperties properties, string name)
    {
        foreach (var prop in properties)
        {
            Indent().AppendLine($"private readonly {prop.Value.type}? _{prop.Key.LowerFirstChar()};");
            Indent().AppendLine($"""public partial {prop.Value.type} {prop.Key} => _{prop.Key.LowerFirstChar()} ?? throw new Jacobi.ValueObject.ValueObjectException("ValueObject '{name}' was not initialized with a valid value for properties '{prop.Key}'.");""");
        }
        return this;
    }

    public CodeBuilder OverrideEqualsAndGetHashCode(string name)
    {
        Indent().AppendLine($"public override bool Equals(object? obj) => obj is {name} && Equals(({name})obj);");
        Indent().AppendLine($"public override int GetHashCode() => Value.GetHashCode();");
        return this;
    }

    public CodeBuilder OverrideEqualsAndGetHashCode(IProperties properties, string name)
    {
        Indent().AppendLine($"public override bool Equals(object? obj) => obj is {name} && Equals(({name})obj);");
        Indent().Append($"public override int GetHashCode() => System.HashCode.Combine(")
            .AppendPropertiesAsArguments(properties, asMembers: true)
            .AppendLine(");");
        return this;
    }

    public CodeBuilder ImplicitFrom(string name, string datatype)
    {
        Indent().AppendLine($"public static implicit operator {name}({datatype} value) => new(value);");
        return this;
    }
    public CodeBuilder ImplicitAs(string name, string datatype)
    {
        Indent().AppendLine($"public static implicit operator {datatype}({name} value) => value.Value;");
        return this;
    }
    public CodeBuilder ExplicitFrom(string name, string datatype, bool isPartial)
    {
        Indent().Append("public static ")
            .Append(isPartial ? "partial " : "")
            .AppendLine($"{name} From({datatype} value) => new(value);");
        return this;
    }

    public CodeBuilder ExplicitFrom(string name, IProperties properties, bool isPartial)
    {
        Indent().Append("public static ")
            .Append(isPartial ? "partial " : "")
            .Append($"{name} From(")
            //.Append(String.Join(", ", properties.Select(p => $"{p.Value.type} {p.Key.LowerFirstChar()}")))
            .AppendPropertiesAsParameters(properties)
            .Append(") => new(")
            //.Append(String.Join(", ", properties.Select(p => $"{p.Key.LowerFirstChar()}")))
            .AppendPropertiesAsArguments(properties, asMembers: false)
            .AppendLine(");");
        return this;
    }

    public CodeBuilder TryCreate(string name, string datatype)
    {
        Indent().AppendLine($$"""public static bool Try({{datatype}} value, out {{name}} valueObject) { if ({{name}}.IsValid(value)) { valueObject = new(value); return true; } valueObject = default; return false; }""");
        return this;
    }

    public CodeBuilder TryCreate(IProperties properties, string name)
    {
        Indent().Append("public static bool Try(")
            .AppendPropertiesAsParameters(properties)
            .Append($$""", out {{name}} valueObject) { if ({{name}}.IsValid(""")
            .AppendPropertiesAsArguments(properties, asMembers: false)
            .Append(")) { valueObject = new(")
            .AppendPropertiesAsArguments(properties, asMembers: false)
            .AppendLine("); return true; } valueObject = default; return false; }")
            ;
        return this;
    }

    public CodeBuilder Deconstruct(IProperties properties)
    {
        Indent().Append("public void Deconstruct(")
            .AppendPropertiesAsOutParameters(properties)
            .Append(") { ")
            .AppendPropertiesAssignments(properties, toPrivates: false)
            .AppendLine(" }")
            ;
        return this;
    }

    public CodeBuilder ValueToString()
    {
        Indent().AppendLine($"public override string ToString() => Value.ToString();");
        return this;
    }

    public CodeBuilder ObjectToString(IProperties properties, string name)
    {
        Indent().Append($"public override string ToString() => $\"{name} {{{{")
            .Append(String.Join(", ", properties.Select(p => $"{p.Key} = {{{p.Key}}}")))
            .AppendLine("}}\";");
        return this;
    }

    public CodeBuilder AddInterfaceImplementations(string name, string datatype)
    {
        if ((_interfaces & CodeBuilderInterfaces.IEquatableStruct) != 0)
        {
            Indent().AppendLine($"public bool Equals({name} value) => Value.Equals(value.Value);");
            Indent().AppendLine($"public static bool operator ==({name} valueObject, {name} value) => valueObject.Equals(value);");
            Indent().AppendLine($"public static bool operator !=({name} valueObject, {name} value) => !valueObject.Equals(value);");
        }

        if ((_interfaces & CodeBuilderInterfaces.IEquatableValue) != 0)
        {
            Indent().AppendLine($"public bool Equals({datatype} value) => Value.Equals(value);");
            Indent().AppendLine($"public static bool operator ==({name} valueObject, {datatype} value) => valueObject.Equals(value);");
            Indent().AppendLine($"public static bool operator !=({name} valueObject, {datatype} value) => !valueObject.Equals(value);");
        }

        if ((_interfaces & CodeBuilderInterfaces.IComparableStruct) != 0)
        {
            Indent().AppendLine($"public int CompareTo({name} value) => Value.CompareTo(value.Value);");
            Indent().AppendLine($"public static bool operator >({name} value1, {name} value2) => value1.CompareTo(value2) > 0;");
            Indent().AppendLine($"public static bool operator <({name} value1, {name} value2) => value1.CompareTo(value2) < 0;");
            Indent().AppendLine($"public static bool operator >=({name} value1, {name} value2) => value1.CompareTo(value2) >= 0;");
            Indent().AppendLine($"public static bool operator <=({name} value1, {name} value2) => value1.CompareTo(value2) <= 0;");
        }

        if ((_interfaces & CodeBuilderInterfaces.IComparableValue) != 0)
        {
            Indent().AppendLine($"public int CompareTo({datatype} value) => Value.CompareTo(value);");
            Indent().AppendLine($"public static bool operator >({name} value1, {datatype} value2) => value1.CompareTo(value2) > 0;");
            Indent().AppendLine($"public static bool operator <({name} value1, {datatype} value2) => value1.CompareTo(value2) < 0;");
            Indent().AppendLine($"public static bool operator >=({name} value1, {datatype} value2) => value1.CompareTo(value2) >= 0;");
            Indent().AppendLine($"public static bool operator <=({name} value1, {datatype} value2) => value1.CompareTo(value2) <= 0;");
        }

        if ((_interfaces & CodeBuilderInterfaces.IParsableStruct) != 0)
        {
            Indent().AppendLine($"public static {name} Parse(string? str, System.IFormatProvider? formatProvider) => new({datatype}.Parse(str, formatProvider));");
            Indent().AppendLine($"public static bool TryParse(string? str, System.IFormatProvider? formatProvider, out {name} result)");
            Scope();
            Indent().AppendLine($"if ({datatype}.TryParse(str, formatProvider, out var dtResult)) {{ result = new (dtResult); return true; }}");
            Indent().AppendLine("result = default; return false;");
            EndScope();
        }

        if ((_interfaces & CodeBuilderInterfaces.ISpanParsableStruct) != 0)
        {
            Indent().AppendLine($"public static {name} Parse(System.ReadOnlySpan<char> str, System.IFormatProvider? formatProvider) => new({datatype}.Parse(str, formatProvider));");
            Indent().AppendLine($"public static bool TryParse(System.ReadOnlySpan<char> str, System.IFormatProvider? formatProvider, out {name} result)");
            Scope();
            Indent().AppendLine($"if ({datatype}.TryParse(str, formatProvider, out var dtResult)) {{ result = new (dtResult); return true; }}");
            Indent().AppendLine("result = default; return false;");
            EndScope();
        }

        return this;
    }

    public CodeBuilder AddInterfaceImplementations(IProperties properties, string name)
    {
        if ((_interfaces & CodeBuilderInterfaces.IEquatableStruct) != 0)
        {
            Indent().Append($"public bool Equals({name} value) => ")
                .Append(String.Join(" && ", properties.Select(p => $"{p.Key}.Equals(value.{p.Key})")))
                .AppendLine(";");

            Indent().AppendLine($"public static bool operator ==({name} valueObject, {name} value) => valueObject.Equals(value);");
            Indent().AppendLine($"public static bool operator !=({name} valueObject, {name} value) => !valueObject.Equals(value);");
        }

        return this;
    }

    public string Build()
    {
        foreach (var item in _dedents)
        {
            UnTab().AppendLine(item);
        }
        Indent().AppendLine("#pragma warning restore CS8604 // Possible null reference argument for parameter...");
        _builder.AppendLine("#nullable restore");
        return _builder.ToString();
    }

    public override string ToString()
        => _builder.ToString();

    private StringBuilder Scope(string start = "{", string end = "}")
    {
        Indent().AppendLine(start);
        _indent++;
        _dedents.Push(end);
        return _builder;
    }
    private StringBuilder EndScope()
    {
        _indent--;
        return Indent().AppendLine(_dedents.Pop());
    }

    private StringBuilder Tab()
    {
        _indent++;
        return Indent();
    }
    private StringBuilder UnTab()
    {
        _indent--;
        return Indent();
    }
    private StringBuilder Indent(int extra = 0)
        => _builder.Append(new string(' ', (_indent + extra) * 4));
}

internal enum CodeBuilderInterfaces
{
    None = 0x00,
    IEquatableStruct = 0x01,
    IEquatableValue = 0x02,
    IComparableStruct = 0x04,
    IComparableValue = 0x08,
    IParsableStruct = 0x10,
    ISpanParsableStruct = 0x20,
}

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendPropertiesAsParameters(this StringBuilder builder, IProperties properties)
        => builder.Append(String.Join(", ", properties.Select(p => $"{p.Value.type} {p.Key.LowerFirstChar()}")));
    public static StringBuilder AppendPropertiesAsOutParameters(this StringBuilder builder, IProperties properties)
        => builder.Append(String.Join(", ", properties.Select(p => $"out {p.Value.type} {p.Key.LowerFirstChar()}")));

    public static StringBuilder AppendPropertiesAsArguments(this StringBuilder builder, IProperties properties, bool asMembers)
        => asMembers
            ? builder.Append(String.Join(", ", properties.Select(p => p.Key)))
            : builder.Append(String.Join(", ", properties.Select(p => p.Key.LowerFirstChar())))
        ;

    public static StringBuilder AppendPropertiesAssignments(this StringBuilder builder, IProperties properties, bool toPrivates)
        => toPrivates
            ? builder.Append(String.Join(" ", properties.Select(p => $"_{p.Key.LowerFirstChar()} = {p.Key.LowerFirstChar()};")))
            : builder.Append(String.Join(" ", properties.Select(p => $"{p.Key.LowerFirstChar()} = {p.Key};")))
        ;
}


/*
private partial class {name}JsonConverter : System.Text.Json.Serialization.JsonConverter<{name}>
{
    public override void Write(System.Text.Json.Utf8JsonWriter writer, {name} value, System.Text.Json.JsonSerializerOptions options)
    {
    }
    public override {name} Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
    }
}


private partial class {name}NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanRead { get; }
    public override bool CanWrite { get; }
    public override bool CanConvert(System.Type type);
    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer);
    public override object ReadJson(Newtonsoft.Json.JsonReader reader, System.Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer);
}
 */