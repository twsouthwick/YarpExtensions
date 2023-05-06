namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpUpgradeFeature
{
    public bool IsUpgradableRequest => GetFeature<IHttpUpgradeFeature>().IsUpgradableRequest;

    public Task<Stream> UpgradeAsync() => GetFeature<IHttpUpgradeFeature>().UpgradeAsync();
}
