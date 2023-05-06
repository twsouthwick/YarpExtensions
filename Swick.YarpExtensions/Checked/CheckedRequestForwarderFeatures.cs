using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions.Checked;

internal sealed partial class CheckedRequestForwarderFeatures : IDisposable, IAsyncDisposable, IHttpRequestLifetimeFeature, IMemoryResponseBodyFeature
{
    private readonly CancellationTokenSource _cts;
    private readonly IFeatureCollection _other;
    private readonly ReplayPassThroughStream _responseStream;

    public CheckedRequestForwarderFeatures(HttpContext context)
    {
        _other = context.Features;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        _responseStream = new ReplayPassThroughStream(Stream.Null);
    }

    public void Dispose()
    {
        _cts.Dispose();
        _responseStream.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        _cts.Dispose();
        return _responseStream.DisposeAsync();
    }

    private TFeature GetFeature<TFeature>() => _other.Get<TFeature>()!;

    CancellationToken IHttpRequestLifetimeFeature.RequestAborted
    {
        get => _cts.Token;
        set => throw new NotImplementedException();
    }

    ReadOnlyMemory<byte> IMemoryResponseBodyFeature.Body => _responseStream.Memory;

    void IHttpRequestLifetimeFeature.Abort() => _cts.Cancel();
}
