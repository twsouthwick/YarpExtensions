namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpExtendedConnectFeature
{
    bool IHttpExtendedConnectFeature.IsExtendedConnect => GetFeature<IHttpExtendedConnectFeature>().IsExtendedConnect;

    string? IHttpExtendedConnectFeature.Protocol => GetFeature<IHttpExtendedConnectFeature>().Protocol;

    ValueTask<Stream> IHttpExtendedConnectFeature.AcceptAsync() => GetFeature<IHttpExtendedConnectFeature>().AcceptAsync();

}


