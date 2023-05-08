using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal sealed class CheckedForwarder : IDisposable
{
    private readonly IHttpForwarder _forwarder;
    private readonly HttpMessageInvoker _client;

    public CheckedForwarder(IHttpForwarder forwarder)
    {
        _forwarder = forwarder;

        _client = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            ConnectTimeout = TimeSpan.FromSeconds(15),
        });
    }

    public ValueTask<ForwarderError> ForwardAsync(HttpContext context, string prefix)
        => _forwarder.SendAsync(context, prefix, _client);

    public void Dispose()
    {
        _client.Dispose();
    }
}
