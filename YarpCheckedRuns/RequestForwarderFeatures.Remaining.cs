using Microsoft.AspNetCore.Http.Features;
using System.Collections;
using System.Net;

namespace YarpCheckedRuns;

partial class RequestForwarderFeatures :
    Microsoft.AspNetCore.Http.Features.IHttpRequestLifetimeFeature,
    Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature,
    Microsoft.AspNetCore.Http.Features.IHttpResponseFeature,
    Microsoft.AspNetCore.Http.Features.IHttpResponseTrailersFeature,
    Microsoft.AspNetCore.Http.Features.IHttpUpgradeFeature
{

}
