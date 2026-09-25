using System.Collections;

namespace Jacobi.ValueObject.Generator;

/// <summary>
/// Registers properties in order of appearance.
/// </summary>
internal sealed class PropertiesLookup : IEnumerable<string>
{
    private readonly List<string> _keyOrder = [];
    private readonly Dictionary<string, (string type, bool isStruct)> _lookup = [];

    public void Add(string key, string type, bool isStruct)
    {
        _lookup.Add(key, (type, isStruct));
        _keyOrder.Add(key);
    }

    public bool Contains(string key)
        => _lookup.ContainsKey(key);

    public int Count
        => _lookup.Count;

    public (string type, bool isStruct) this[string key]
    {
        get => _lookup[key];
    }

    public IEnumerable<string> Keys
        => _keyOrder;

    public IEnumerable<string> ValueTypes()
        => _keyOrder.Select(key => _lookup[key].type);

    public IEnumerator<string> GetEnumerator()
        => _keyOrder.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
