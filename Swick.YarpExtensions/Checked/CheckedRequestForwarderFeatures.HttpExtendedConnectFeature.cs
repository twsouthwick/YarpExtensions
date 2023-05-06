namespace Swick.YarpExtensions.Checked;

partial class CheckedRequestForwarderFeatures : IHttpExtendedConnectFeature
{
    bool IHttpExtendedConnectFeature.IsExtendedConnect => GetFeature<IHttpExtendedConnectFeature>().IsExtendedConnect;

    string? IHttpExtendedConnectFeature.Protocol => GetFeature<IHttpExtendedConnectFeature>().Protocol;

    ValueTask<Stream> IHttpExtendedConnectFeature.AcceptAsync() => GetFeature<IHttpExtendedConnectFeature>().AcceptAsync();
}


