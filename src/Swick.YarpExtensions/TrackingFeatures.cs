using System.Collections;

namespace Swick.YarpExtensions;

internal class TrackingFeatures : IFeatureCollection
{
    private readonly IFeatureCollection _other;

    public ICollection<Type> GetTypes { get; } = new HashSet<Type>();

    public ICollection<Type> SetTypes { get; } = new HashSet<Type>();

    public TrackingFeatures(IFeatureCollection features)
    {
        _other = features;
    }

    public object? this[Type key]
    {
        get
        {
            GetTypes.Add(key);
            return _other[key];
        }
        set
        {
            SetTypes.Add(key);
            _other[key] = value;
        }
    }

    public bool IsReadOnly => _other.IsReadOnly;

    public int Revision => _other.Revision;

    public TFeature? Get<TFeature>()
    {
        GetTypes.Add(typeof(TFeature));
        return _other.Get<TFeature>();
    }

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
    {
        return _other.GetEnumerator();
    }

    public void Set<TFeature>(TFeature? instance)
    {
        SetTypes.Add(typeof(TFeature));
        _other.Set(instance);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_other).GetEnumerator();
    }   
}

