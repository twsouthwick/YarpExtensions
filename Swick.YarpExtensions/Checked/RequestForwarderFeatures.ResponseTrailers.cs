namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpResponseTrailersFeature
{
    private IHttpResponseTrailersFeature GetTrailers() => GetFeature<IHttpResponseTrailersFeature>()!;

    IHeaderDictionary IHttpResponseTrailersFeature.Trailers
    {
        get => GetTrailers()?.Trailers ?? ReadOnlyHeaderDictionary.Empty;
        set => throw new NotImplementedException();
    }
}
