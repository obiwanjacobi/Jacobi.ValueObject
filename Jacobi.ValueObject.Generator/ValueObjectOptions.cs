namespace Jacobi.ValueObject.Generator;

/// <summary>
/// Duplicate of ValueObject.ValueObjectOptions
/// </summary>
[Flags]
public enum ValueObjectOptions
{
    None = 0,
    /// <summary>Implicit assignment to initialize a new value object.</summary>
    ImplicitFrom = 0x01,
    /// <summary>Implicit assignment to extract the value.</summary>
    ImplicitAs = 0x02,
    /// <summary>Explicit static (factory) method to initialize a new value object.</summary>
    ExplicitFrom = 0x04,
    /// <summary>Override (record impl. of) ToString() method to extract return the value as string.</summary>
    ToString = 0x08,
    /// <summary>Constructor to initialize a new value object (default).</summary>
    Constructor = 0x10,
    /// <summary>Adds IComparable[T] interface implementations.</summary>
    Comparable = 0x20,
    /// <summary>Adds (Try)Parse methods.</summary>
    Parsable = 0x40,
}

[Flags]
public enum MultiValueObjectOptions
{
    /// <summary>Invalid</summary>
    None = 0,
    /// <summary>Explicit static (factory) method to initialize a new value object.</summary>
    ExplicitFrom = 0x01,
    /// <summary>Constructor to initialize a new value object (default).</summary>
    Constructor = 0x02,
}
