namespace Swick.YarpExtensions.Checked;

internal sealed partial class RequestForwarderFeatures : IDisposable, IHttpRequestLifetimeFeature
{
    private readonly CancellationTokenSource _cts;
    private readonly IFeatureCollection _other;
    private readonly Stream _responseStream;

    public RequestForwarderFeatures(HttpContext context)
    {
        _other = context.Features;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        _responseStream = new BufferingReadWriteStream();
    }

    public void Dispose()
    {
        _cts.Dispose();
        _responseStream.Dispose();
    }

    private TFeature GetFeature<TFeature>() => _other.Get<TFeature>()!;

    CancellationToken IHttpRequestLifetimeFeature.RequestAborted
    {
        get => _cts.Token;
        set => throw new NotImplementedException();
    }

    void IHttpRequestLifetimeFeature.Abort() => _cts.Cancel();
}
