using Microsoft.AspNetCore.Http.Features;
using System.Collections;
using System.Net;

namespace YarpCheckedRuns;

partial class RequestForwarderFeatures :
    Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature
{
    private IHttpConnectionFeature GetConnectionFeature() => _other.Get<IHttpConnectionFeature>()!;

    string IHttpConnectionFeature.ConnectionId
    {
        get => GetConnectionFeature().ConnectionId;
        set => throw new NotImplementedException();
    }

    IPAddress? IHttpConnectionFeature.RemoteIpAddress
    {
        get => GetConnectionFeature().RemoteIpAddress;
        set => throw new NotImplementedException();
    }

    IPAddress? IHttpConnectionFeature.LocalIpAddress
    {
        get => GetConnectionFeature().LocalIpAddress;
        set => throw new NotImplementedException();
    }

    int IHttpConnectionFeature.RemotePort
    {
        get => GetConnectionFeature().RemotePort;
        set => throw new NotImplementedException();
    }

    int IHttpConnectionFeature.LocalPort
    {
        get => GetConnectionFeature().LocalPort;
        set => throw new NotImplementedException();
    }
}


