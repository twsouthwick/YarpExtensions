namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpUpgradeFeature
{
    private IHttpUpgradeFeature GetUpgrade() => GetFeature<IHttpUpgradeFeature>()!;

    public bool IsUpgradableRequest => GetUpgrade().IsUpgradableRequest;

    public Task<Stream> UpgradeAsync() => GetUpgrade().UpgradeAsync();
}
