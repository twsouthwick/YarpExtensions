namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures
{
    private readonly IFeatureCollection _other;

    public RequestForwarderFeatures(HttpContext context)
    {
        _other = context.Features;

        InitializeResponse(context);
    }

    private TFeature GetFeature<TFeature>() => _other.Get<TFeature>()!;
}
