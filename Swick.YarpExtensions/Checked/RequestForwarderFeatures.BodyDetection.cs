namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpRequestBodyDetectionFeature
{
    bool IHttpRequestBodyDetectionFeature.CanHaveBody => GetFeature<IHttpRequestBodyDetectionFeature>()!.CanHaveBody;
}
