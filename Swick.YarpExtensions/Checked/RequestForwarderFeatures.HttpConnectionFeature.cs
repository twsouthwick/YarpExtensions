using System.Net;

namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpConnectionFeature
{
    string IHttpConnectionFeature.ConnectionId
    {
        get => GetFeature<IHttpConnectionFeature>().ConnectionId;
        set => throw new NotImplementedException();
    }

    IPAddress? IHttpConnectionFeature.RemoteIpAddress
    {
        get => GetFeature<IHttpConnectionFeature>().RemoteIpAddress;
        set => throw new NotImplementedException();
    }

    IPAddress? IHttpConnectionFeature.LocalIpAddress
    {
        get => GetFeature<IHttpConnectionFeature>().LocalIpAddress;
        set => throw new NotImplementedException();
    }

    int IHttpConnectionFeature.RemotePort
    {
        get => GetFeature<IHttpConnectionFeature>().RemotePort;
        set => throw new NotImplementedException();
    }

    int IHttpConnectionFeature.LocalPort
    {
        get => GetFeature<IHttpConnectionFeature>().LocalPort;
        set => throw new NotImplementedException();
    }
}


