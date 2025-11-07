namespace Jacobi.ValueObject;

/// <summary>
/// Identifies the record struct as a value object.
/// </summary>
/// <remarks>
/// <code>
/// [ValueObject(typeof(string))]
/// public partial record struct YourValueObject;
/// </code>
/// <code>
/// [ValueObject(typeof(int), Options = ValueObjectOptions.ExplicitFrom)]
/// public partial record struct YourValueObject;
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ValueObjectAttribute : Attribute
{
    /// <summary>
    /// Constructs a new attribute.
    /// </summary>
    /// <param name="dataType">The data type of the value object.</param>
    /// <remarks>By default the Constructor option is set.</remarks>
    public ValueObjectAttribute(Type dataType)
    {
        DataType = dataType;
        Options = ValueObjectOptions.Constructor;
    }

    /// <summary>
    /// Gets the datatype of the value object.
    /// </summary>
    public Type DataType { get; }

    /// <summary>
    /// Gets or sets the configuration options for the value object.
    /// </summary>
    public ValueObjectOptions Options { get; set; }
}

/// <summary>
/// Identifies the record struct as a value object.
/// </summary>
/// <typeparam name="T">The datatype of the value object.</typeparam>
/// <remarks>
/// <code>
/// [ValueObject<string>]
/// public partial record struct YourValueObject;
/// </code>
/// <code>
/// [ValueObject<int>(Options = ValueObjectOptions.ExplicitFrom)]
/// public partial record struct YourValueObject;
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ValueObjectAttribute<T> : Attribute
{
    /// <summary>
    /// Constructs a new attribute.
    /// </summary>
    /// <remarks>By default the Constructor option is set.</remarks>
    public ValueObjectAttribute()
    {
        DataType = typeof(T);
        Options = ValueObjectOptions.Constructor;
    }

    /// <summary>
    /// Gets the datatype of the value object.
    /// </summary>
    public Type DataType { get; }

    /// <summary>
    /// Gets or sets the configuration options for the value object.
    /// </summary>
    public ValueObjectOptions Options { get; set; }
}
