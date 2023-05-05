using Microsoft.AspNetCore.Http.Features;

namespace YarpCheckedRuns;

partial class RequestForwarderFeatures : IHttpExtendedConnectFeature
{
    private IHttpExtendedConnectFeature GetExtendedConnectFeature() => _other.Get<IHttpExtendedConnectFeature>()!;

    bool IHttpExtendedConnectFeature.IsExtendedConnect => GetExtendedConnectFeature().IsExtendedConnect;

    string? IHttpExtendedConnectFeature.Protocol => GetExtendedConnectFeature().Protocol;

    ValueTask<Stream> IHttpExtendedConnectFeature.AcceptAsync() => GetExtendedConnectFeature().AcceptAsync();

}


