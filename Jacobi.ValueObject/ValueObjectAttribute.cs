namespace Jacobi.ValueObject;

/// <summary>
/// Identifies the struct or record struct as a (single) value object.
/// </summary>
/// <example>
/// <code>
/// [ValueObject(typeof(string))]
/// public partial record struct YourValueObject;
/// </code>
/// <code>
/// [ValueObject(typeof(int), Options = ValueObjectOptions.ExplicitFrom)]
/// public partial struct YourValueObject;
/// </code>
/// </example>
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
/// <example>
/// <code>
/// [ValueObject&lt;string&gt;]
/// public partial record struct YourValueObject;
/// </code>
/// <code>
/// [ValueObject&lt;int7gt;(ValueObjectOptions.ExplicitFrom)]
/// public partial record struct YourValueObject;
/// </code>
/// </example>
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
    /// Constructs a new attribute with options.
    /// </summary>
    public ValueObjectAttribute(ValueObjectOptions options)
    {
        DataType = typeof(T);
        Options = options;
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
