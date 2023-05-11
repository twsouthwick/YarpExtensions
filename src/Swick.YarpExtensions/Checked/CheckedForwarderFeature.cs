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

    public CheckedForwarderFeature(HttpContext mainRequest, RequestDelegate comparison, CheckedForwarder forwarder, string prefix)
    {
        _mainRequest = mainRequest;
        _comparison = comparison;
        _forwarder = forwarder;
        _prefix = prefix;

        Logger = mainRequest.RequestServices.GetRequiredService<ILogger<CheckedForwarderFeature>>();

        Context = new DefaultHttpContext();

        var feature = new ReadOnlyRequestFeatures(mainRequest.Features, Context.Features.GetRequiredFeature<IHttpResponseFeature>());

        // Ensure the context we'll use for forwarding doesn't modify the original request but forwards a readonly view of things
        Context.Features.Set<IHttpRequestFeature>(feature);
        Context.Features.Set<IHttpRequestBodyDetectionFeature>(feature);

        // This will only change the OnCompleted callbacks to ensure they get called as expected on the original context
        Context.Features.Set<IHttpResponseFeature>(feature);

        // Connect cancellation token
        Context.RequestAborted = mainRequest.RequestAborted;
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
}
