namespace Swick.YarpExtensions.Checked;

partial class CheckedRequestForwarderFeatures : IHttpRequestBodyDetectionFeature
{
    bool IHttpRequestBodyDetectionFeature.CanHaveBody => GetFeature<IHttpRequestBodyDetectionFeature>()!.CanHaveBody;
}
