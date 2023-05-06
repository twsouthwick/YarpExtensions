namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpRequestFeature
{
    private FeatureReference<IHttpRequestFeature> _request = FeatureReference<IHttpRequestFeature>.Default;
    private IHeaderDictionary? _requestHeaders;

    private IHttpRequestFeature RequestFeature => _request.Fetch(_other)!;

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
}
