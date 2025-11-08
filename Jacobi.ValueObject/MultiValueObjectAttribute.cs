namespace Jacobi.ValueObject;

/// <summary>
/// Identifies the record struct as a value object.
/// </summary>
/// <remarks>
/// <code>
/// [MultiValueObject)]
/// public partial struct YourValueObject
/// {
///     public partial int Id { get; }
///     public partial string Name { get; }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class MultiValueObjectAttribute : Attribute
{
    /// <summary>
    /// Constructs a new attribute.
    /// </summary>
    /// <remarks>By default the Constructor option is set.</remarks>
    public MultiValueObjectAttribute()
    {
        Options = MultiValueObjectOptions.Constructor;
    }

    // <summary>
    /// Gets or sets the configuration options for the value object.
    /// </summary>
    public MultiValueObjectOptions Options { get; set; }
}
