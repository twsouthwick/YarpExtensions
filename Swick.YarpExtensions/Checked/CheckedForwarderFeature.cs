using Swick.YarpExtensions.Features;
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

    public ForwarderError Error { get; set; }

    public async ValueTask ForwardAsync()
    {
        using (new ResetStreamPosition(Context.Request.Body, 0))
        {
            Error = await _forwarder.ForwardAsync(Context, _prefix);
        }
    }

    public ValueTask CompareAsync() => _differ.CompareAsync(_mainRequest, Context, Error);
}
