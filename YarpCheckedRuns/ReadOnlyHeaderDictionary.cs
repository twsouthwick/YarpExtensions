using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace YarpCheckedRuns;

internal sealed class ReadOnlyHeaderDictionary : IHeaderDictionary
{
    private readonly IHeaderDictionary _other;

    public ReadOnlyHeaderDictionary(IHeaderDictionary other)
    {
        _other = other;
    }

    public StringValues this[string key]
    {
        get => _other[key];
        set => throw new NotSupportedException();
    }

    public long? ContentLength
    {
        get => _other.ContentLength;
        set => throw new NotSupportedException();
    }

    public ICollection<string> Keys => _other.Keys;

    public ICollection<StringValues> Values => _other.Values;

    public int Count => _other.Count;

    public bool IsReadOnly => true;

    public void Add(string key, StringValues value) => throw new NotSupportedException();

    public void Add(KeyValuePair<string, StringValues> item) => throw new NotSupportedException();

    public void Clear() => throw new NotSupportedException();

    public bool Contains(KeyValuePair<string, StringValues> item)
    {
        return _other.Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return _other.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {
        _other.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _other.GetEnumerator();
    }

    public bool Remove(string key) => throw new NotSupportedException();

    public bool Remove(KeyValuePair<string, StringValues> item) => throw new NotSupportedException();

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value)
    {
        return _other.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_other).GetEnumerator();
    }
}

