namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpRequestLifetimeFeature
{
    private IHttpRequestLifetimeFeature GetLifetime() => GetFeature<IHttpRequestLifetimeFeature>()!;

    CancellationToken IHttpRequestLifetimeFeature.RequestAborted
    {
        get => GetLifetime().RequestAborted;
        set => throw new NotImplementedException();
    }

    void IHttpRequestLifetimeFeature.Abort() => GetLifetime().Abort();
}
