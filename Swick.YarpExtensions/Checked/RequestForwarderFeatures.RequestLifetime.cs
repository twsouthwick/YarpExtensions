namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures : IHttpRequestLifetimeFeature
{
    CancellationToken IHttpRequestLifetimeFeature.RequestAborted
    {
        get => GetFeature<IHttpRequestLifetimeFeature>().RequestAborted;
        set => throw new NotImplementedException();
    }

    void IHttpRequestLifetimeFeature.Abort() => GetFeature<IHttpRequestLifetimeFeature>().Abort();
}
