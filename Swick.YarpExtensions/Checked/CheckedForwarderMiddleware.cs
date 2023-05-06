using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal sealed class CheckedForwarderMiddleware : IDisposable
{
    private readonly IHttpForwarder _forwarder;
    private readonly HttpContextDiffer _differ;
    private readonly RequestDelegate _next;
    private readonly HttpMessageInvoker _client;

    public CheckedForwarderMiddleware(IHttpForwarder forwarder, IServiceProvider services, RequestDelegate next)
    {
        _forwarder = forwarder;
        _differ = ActivatorUtilities.CreateInstance<HttpContextDiffer>(services);
        _next = next;

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

    public Task InvokeAsync(HttpContext context)
    {
        if (context.GetCheckedMetadata() is { } metadata)
        {
            return InvokeForwarder(context, metadata);
        }
        else
        {
            return _next(context);
        }
    }

    private async Task InvokeForwarder(HttpContext context, CheckedYarpMetadata metadata)
    {
        using (new ResetStreamPosition(context.Request.Body))
        {
            var feature = new CheckedForwarderFeature(context, _differ);

            feature.Error = await _forwarder.SendAsync(feature.Context, metadata.Destination, _client);

            context.Features.Set<ICheckedForwarderFeature>(feature);
        }

        await _next(context);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
