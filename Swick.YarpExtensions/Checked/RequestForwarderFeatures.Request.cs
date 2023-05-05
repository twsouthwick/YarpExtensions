namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpRequestFeature
{
    private IHttpRequestFeature GetRequest() => GetFeature<IHttpRequestFeature>()!;

    string IHttpRequestFeature.Protocol
    {
        get => GetRequest().Protocol;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.Scheme
    {
        get => GetRequest().Scheme;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.Method
    {
        get => GetRequest().Method;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.PathBase
    {
        get => GetRequest().PathBase;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.Path
    {
        get => GetRequest().Path;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.QueryString
    {
        get => GetRequest().QueryString;
        set => throw new NotImplementedException();
    }

    string IHttpRequestFeature.RawTarget
    {
        get => GetRequest().RawTarget;
        set => throw new NotImplementedException();
    }

    private IHeaderDictionary? _requestHeaders;

    IHeaderDictionary IHttpRequestFeature.Headers
    {
        get
        {
            if (_requestHeaders is null)
            {
                _requestHeaders = GetRequest().Headers;

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
        get => GetRequest().Body;
        set => throw new NotImplementedException();
    }
}
