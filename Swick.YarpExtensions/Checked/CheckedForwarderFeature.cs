using Swick.YarpExtensions.Features;
using System.Diagnostics.CodeAnalysis;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal sealed class CheckedForwarderFeature : ICheckedForwarderFeature
{
    private readonly HttpContext _mainRequest;
    private readonly HttpContextDiffer _differ;
    private readonly CheckedForwarder _forwarder;
    private readonly string _prefix;

    public CheckedForwarderFeature(HttpContext mainRequest, HttpContextDiffer differ, CheckedForwarder forwarder, string prefix)
    {
        _mainRequest = mainRequest;
        _differ = differ;
        _forwarder = forwarder;
        _prefix = prefix;

        var features = new CheckedRequestForwarderFeatures(mainRequest);
        Context = new DefaultHttpContext(features);

        mainRequest.Response.RegisterForDispose(features);
    }

    public HttpContext Context { get; }

    public ForwarderError? Error { get; set; }

    public async ValueTask ForwardAsync()
    {
        if (Error is not null)
        {
            throw new InvalidOperationException("Request has already been forwarded.");
        }

        using (new ResetStreamPosition(Context.Request.Body, 0))
        {
            Error = await _forwarder.ForwardAsync(Context, _prefix);
        }
    }

    public async ValueTask CompareAsync()
    {
        if (Error is null)
        {
            await ForwardAsync();
        }

        await _differ.CompareAsync(_mainRequest, Context, Error.GetValueOrDefault());
    }
}
