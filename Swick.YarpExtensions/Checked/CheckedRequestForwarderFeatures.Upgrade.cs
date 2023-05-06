namespace Swick.YarpExtensions.Checked;

partial class CheckedRequestForwarderFeatures : IHttpUpgradeFeature
{
    public bool IsUpgradableRequest => GetFeature<IHttpUpgradeFeature>().IsUpgradableRequest;

    public Task<Stream> UpgradeAsync() => GetFeature<IHttpUpgradeFeature>().UpgradeAsync();
}
