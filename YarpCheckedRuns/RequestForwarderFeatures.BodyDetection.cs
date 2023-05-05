using Microsoft.AspNetCore.Http.Features;
using System.Collections;
using System.Net;

namespace YarpCheckedRuns;

partial class RequestForwarderFeatures : IHttpRequestBodyDetectionFeature
{
    bool IHttpRequestBodyDetectionFeature.CanHaveBody => _other.Get<IHttpRequestBodyDetectionFeature>()!.CanHaveBody;
}
