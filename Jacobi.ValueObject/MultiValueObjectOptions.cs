namespace Jacobi.ValueObject;

/// <summary>
/// Fine-tuning what to generate for a multi value object.
/// </summary>
[Flags]
public enum MultiValueObjectOptions
{
    /// <summary>Invalid</summary>
    None = 0,
    /// <summary>Explicit static (factory) method to initialize a new value object.</summary>
    ExplicitFrom = 0x01,
    /// <summary>Constructor to initialize a new value object (default).</summary>
    Constructor = 0x02,
    /// <summary>Add deconstruct support.</summary>
    Deconstruct = 0x04,
    /// <summary>Add System.Text.Json serialization support.</summary>
    Json = 0x08,
    /// <summary>Add Newtonsoft.Json serialization support.</summary>
    NewtonsoftJson = 0x10,
}
