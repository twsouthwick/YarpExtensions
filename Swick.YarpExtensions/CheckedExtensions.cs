using Microsoft.AspNetCore.Builder;
using Swick.YarpExtensions.Checked;
using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions;

public static class CheckedExtensions
{
    public static void UseCheckedForwarder(this IApplicationBuilder app)
    {
        app.UseMiddleware<CheckedForwarderMiddleware>();
    }

    public static IEndpointConventionBuilder WithCheckedYarp(this IEndpointConventionBuilder builder, string destination, ICheckedYarpComparer comparer)
        => builder.WithMetadata(new CheckedYarpMetadata(comparer, destination));

    private sealed class CheckedForwarderMiddleware : IDisposable
    {
        private readonly IHttpForwarder _forwarder;
        private readonly RequestDelegate _next;
        private readonly HttpMessageInvoker _client;

        public CheckedForwarderMiddleware(IHttpForwarder forwarder, RequestDelegate next)
        {
            _forwarder = forwarder;
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
            if (context.GetEndpoint()?.Metadata.GetMetadata<CheckedYarpMetadata>() is { } metadata)
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
            var resultContext = new DefaultHttpContext(new RequestForwarderFeatures(context));
            var error = await _forwarder.SendAsync(resultContext, metadata.Destination, _client);

            var stream = new MemoryStream();
            var current = context.Response.Body;
            context.Response.Body = stream;

            await _next(context);

            await metadata.Comparer.CompareAsync(context, resultContext, error);

            stream.Position = 0;
            await stream.CopyToAsync(current);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}