namespace Jacobi.ValueObject;

/// <summary>
/// Identifies the struct as a value object.
/// </summary>
/// <remarks>
/// <code>
/// [ValueObject(typeof(string))]
/// public partial record struct 'YourValueObject'();
/// </code>
/// <code>
/// [ValueObject(typeof(string), Options = ValueObjectOptions.ExplicitFrom)]
/// public partial record struct 'YourValueObject'();
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ValueObjectAttribute : Attribute
{
    public ValueObjectAttribute(Type dataType)
    {
        DataType = dataType;
        Options = ValueObjectOptions.Constructor;
    }

    public Type DataType { get; }
    public ValueObjectOptions Options { get; set; }
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ValueObjectAttribute<T> : Attribute
{
    public ValueObjectAttribute()
    {
        DataType = typeof(T);
        Options = ValueObjectOptions.Constructor;
    }

    public Type DataType { get; }
    public ValueObjectOptions Options { get; set; }
}
