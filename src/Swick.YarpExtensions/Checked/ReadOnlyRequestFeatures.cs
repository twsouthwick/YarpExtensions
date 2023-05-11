namespace Swick.YarpExtensions.Checked;

internal sealed class ReadOnlyRequestFeatures : IHttpRequestFeature, IHttpRequestBodyDetectionFeature, IHttpResponseFeature
{
    private readonly IFeatureCollection _features;
    private readonly IHttpResponseFeature _response;

    private FeatureReference<IHttpRequestFeature> _request = FeatureReference<IHttpRequestFeature>.Default;
    private IHeaderDictionary? _requestHeaders;

    public ReadOnlyRequestFeatures(IFeatureCollection features, IHttpResponseFeature response)
    {
        _features = features;
        _response = response;
    }

    private IHttpRequestFeature RequestFeature => _request.Fetch(_features)!;

    bool IHttpRequestBodyDetectionFeature.CanHaveBody => _features.GetRequiredFeature<IHttpRequestBodyDetectionFeature>().CanHaveBody;

    string IHttpRequestFeature.Protocol
    {
        get => RequestFeature.Protocol;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.Scheme
    {
        get => RequestFeature.Scheme;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.Method
    {
        get => RequestFeature.Method;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.PathBase
    {
        get => RequestFeature.PathBase;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.Path
    {
        get => RequestFeature.Path;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.QueryString
    {
        get => RequestFeature.QueryString;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.RawTarget
    {
        get => RequestFeature.RawTarget;
        set => throw new NotImplementedException();
    }

    IHeaderDictionary IHttpRequestFeature.Headers
    {
        get
        {
            if (_requestHeaders is null)
            {
                _requestHeaders = RequestFeature.Headers;

                if (!_requestHeaders.IsReadOnly)
                {
                    _requestHeaders = new ReadOnlyHeaderDictionary(_requestHeaders);
                }
            }

            return _requestHeaders;
        }
        set => throw new NotImplementedException();
    }

    Stream IHttpRequestFeature.Body
    {
        get => RequestFeature.Body;
        set => throw new NotImplementedException();
    }

    int IHttpResponseFeature.StatusCode
    {
        get => _response.StatusCode;
        set => _response.StatusCode = value;
    }

    string? IHttpResponseFeature.ReasonPhrase
    {
        get => _response.ReasonPhrase;
        set => _response.ReasonPhrase = value;
    }

    IHeaderDictionary IHttpResponseFeature.Headers
    {
        get => _response.Headers;
        set => _response.Headers = value;
    }

    [Obsolete]
    Stream IHttpResponseFeature.Body
    {
        get => _response.Body;
        set => _response.Body = value;
    }

    bool IHttpResponseFeature.HasStarted => _response.HasStarted;

    void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
    {
        // We intentionally on redirect these calls to the original context so that things like RegisterDispose work
        _features.GetRequiredFeature<IHttpResponseFeature>().OnCompleted(callback, state);
    }

    void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
    {
        _response.OnStarting(callback, state);
    }
}
