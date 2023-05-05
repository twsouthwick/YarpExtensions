using Microsoft.AspNetCore.Http.Features;
using System.Collections;
using System.Net;

namespace YarpCheckedRuns;

partial class RequestForwarderFeatures
{
    private readonly IFeatureCollection _other;

    public RequestForwarderFeatures(IFeatureCollection other) => _other = other;
}
