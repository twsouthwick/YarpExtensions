using System.Collections;

namespace Swick.YarpExtensions.Checked;

public partial class RequestForwarderFeatures : IFeatureCollection
{
    private static readonly HashSet<Type> _supportedFeatures = typeof(RequestForwarderFeatures)
        .GetType()
        .GetInterfaces()
        .ToHashSet();

    object? IFeatureCollection.this[Type key]
    {
        get
        {
            if (_supportedFeatures.Contains(key))
            {
                return this;
            }

            return null;
        }
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
            if (((IFeatureCollection)this)[f] is { } value)
            {
                yield return new(f, value);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetKeyValues().GetEnumerator();

    void IFeatureCollection.Set<TFeature>(TFeature? instance) where TFeature : default
    {
        throw new NotSupportedException();
    }
}

