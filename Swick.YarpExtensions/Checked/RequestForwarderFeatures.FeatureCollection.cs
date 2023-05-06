using System.Collections;

namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IFeatureCollection
{
    private static readonly HashSet<Type> _supportedFeatures = typeof(RequestForwarderFeatures)
        .GetType()
        .GetInterfaces()
        .Where(t => t != typeof(IFeatureCollection))
        .ToHashSet();

    object? IFeatureCollection.this[Type key]
    {
        get => _supportedFeatures.Contains(key) ? this : default;
        set => throw new NotSupportedException();
    }

    bool IFeatureCollection.IsReadOnly => true;

    int IFeatureCollection.Revision => 0;

    TFeature? IFeatureCollection.Get<TFeature>() where TFeature : default
    {
        if (typeof(TFeature) == typeof(IFeatureCollection))
        {
            return default;
        }

        if (this is TFeature t)
        {
            return t;
        }

        return default;
    }

    IEnumerator<KeyValuePair<Type, object>> IEnumerable<KeyValuePair<Type, object>>.GetEnumerator()
        => GetKeyValues().GetEnumerator();

    private IEnumerable<KeyValuePair<Type, object>> GetKeyValues()
    {
        foreach (var f in _supportedFeatures)
        {
            yield return new(f, this);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetKeyValues().GetEnumerator();

    void IFeatureCollection.Set<TFeature>(TFeature? instance) where TFeature : default
    {
        throw new NotSupportedException();
    }
}

