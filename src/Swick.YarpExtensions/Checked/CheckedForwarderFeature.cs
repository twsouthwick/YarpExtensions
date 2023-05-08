using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swick.YarpExtensions.Features;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal sealed class CheckedForwarderFeature : ICheckedForwarderFeature
{
    private readonly HttpContext _mainRequest;
    private readonly RequestDelegate _comparison;
    private readonly CheckedForwarder _forwarder;
    private readonly string _prefix;

    public CheckedForwarderFeature(HttpContext mainRequest, RequestDelegate comparison,  CheckedForwarder forwarder, string prefix)
    {
        _mainRequest = mainRequest;
        _comparison = comparison;
        _forwarder = forwarder;
        _prefix = prefix;

        Logger = mainRequest.RequestServices.GetRequiredService<ILogger<CheckedForwarderFeature>>();

        Context = new DefaultHttpContext();

        ReadOnlyRequestFeatures.Add(mainRequest, Context);

        Context.Response.BufferResponseStreamToMemory();
    }

    public HttpContext Context { get; }

    public ForwarderError? Status { get; set; }

    public ILogger Logger { get; }

    public async ValueTask ForwardAsync()
    {
        if (Status is not null)
        {
            throw new InvalidOperationException("Request has already been forwarded.");
        }

        using (new ResetStreamPosition(Context.Request.Body, 0))
        {
            Status = await _forwarder.ForwardAsync(Context, _prefix);
        }
    }

    public async ValueTask CompareAsync()
    {
        if (Status is null)
        {
            await ForwardAsync();
        }

        await _comparison(_mainRequest);
    }

    private sealed class ReadOnlyRequestFeatures : IHttpRequestFeature, IHttpRequestBodyDetectionFeature
    {
        private readonly IFeatureCollection _features;

        private FeatureReference<IHttpRequestFeature> _request = FeatureReference<IHttpRequestFeature>.Default;
        private IHeaderDictionary? _requestHeaders;

        public ReadOnlyRequestFeatures(IFeatureCollection features)
        {
            _features = features;
        }

        public static void Add(HttpContext source, HttpContext destination)
        {
            var instance = new ReadOnlyRequestFeatures(source.Features);

            destination.Features.Set<IHttpRequestFeature>(instance);
            destination.Features.Set<IHttpRequestBodyDetectionFeature>(instance);
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
    }
}
