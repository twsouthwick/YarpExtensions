using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal sealed class CheckedForwarderFeature : ICheckedForwarderFeature
{
    private readonly HttpContext _mainRequest;
    private readonly HttpContextDiffer _differ;

    public CheckedForwarderFeature(HttpContext mainRequest, HttpContextDiffer differ)
    {
        _mainRequest = mainRequest;
        _differ = differ;

        var features = new RequestForwarderFeatures(mainRequest);
        Context = new DefaultHttpContext(features);

        mainRequest.Response.RegisterForDispose(features);
    }

    public HttpContext Context { get; }

    public ForwarderError Error { get; set; }

    public ValueTask CompareAsync() => _differ.CompareAsync(_mainRequest, Context, Error);
}
