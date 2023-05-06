namespace Swick.YarpExtensions.Checked;

internal sealed partial class RequestForwarderFeatures : IDisposable, IHttpRequestLifetimeFeature
{
    private readonly CancellationTokenSource _cts;
    private readonly IFeatureCollection _other;

    public RequestForwarderFeatures(HttpContext context)
    {
        _other = context.Features;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);

        InitializeResponse(context);
    }

    public void Dispose()
    {
        _cts.Dispose();
    }

    private TFeature GetFeature<TFeature>() => _other.Get<TFeature>()!;

    CancellationToken IHttpRequestLifetimeFeature.RequestAborted
    {
        get => _cts.Token;
        set => throw new NotImplementedException();
    }

    void IHttpRequestLifetimeFeature.Abort() => _cts.Cancel();
}
