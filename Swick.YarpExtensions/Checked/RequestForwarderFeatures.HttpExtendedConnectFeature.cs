namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpExtendedConnectFeature
{
    private IHttpExtendedConnectFeature GetExtendedConnectFeature() => GetFeature<IHttpExtendedConnectFeature>()!;

    bool IHttpExtendedConnectFeature.IsExtendedConnect => GetExtendedConnectFeature().IsExtendedConnect;

    string? IHttpExtendedConnectFeature.Protocol => GetExtendedConnectFeature().Protocol;

    ValueTask<Stream> IHttpExtendedConnectFeature.AcceptAsync() => GetExtendedConnectFeature().AcceptAsync();

}


