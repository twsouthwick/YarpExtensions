using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Swick.YarpExtensions.Comparer;
using Swick.YarpExtensions.Features;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions;

public static class ContextComparerExtensions
{
    public static void UseWhen(this IContextComparerBuilder builder, Func<HttpContext, ValueTask<bool>> predicate)
    {
        builder.Request.Use(async (ctx, next) =>
        {
            if (await predicate(ctx))
            {
                await next(ctx);
            }
            else
            {
                ctx.Features.Set<ICheckedForwarderFeature>(null);
            }
        });
    }

    public static void UseJsonBody<T>(this IContextComparerBuilder builder)
        => builder.UseJsonBody<T>(EqualityComparer<T>.Default);

    public static void UseJsonBody<T>(this IContextComparerBuilder builder, IEqualityComparer<T> comparer, JsonSerializerOptions? options = null)
    {
        // TODO: Should we mimic settings from MVC or minimal APIs?
        options ??= new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };

        builder.UseResponseBuffering();

        builder.Comparison.UseForwardedContext((context, forwarded) =>
        {
            if (TryGetMemory(context, forwarded, out var localMemory, out var forwardedMemory))
            {
                var responseObj = JsonSerializer.Deserialize<T>(localMemory.Span, options);
                var forwardedObj = JsonSerializer.Deserialize<T>(forwardedMemory.Span, options);

                if (!comparer.Equals(responseObj, forwardedObj))
                {
                    forwarded.Logger.LogWarning("Local response and forwarded response do not match");
                }
            }
        });
    }

    internal static void UseForwarderChecks(this IContextComparerBuilder builder)
    {
        builder.Comparison.Use(async (ctx, next) =>
        {
            if (ctx.Features.Get<ICheckedForwarderFeature>() is { } feature)
            {
                // Verify forwarder has run successfully
                if (feature.Status != ForwarderError.None)
                {
                    feature.Logger.LogWarning("Forwarder failed with {ForwarderStatus}", feature.Status);
                }

                // Skip if a 4xx error
                else if (feature.Context.Response.StatusCode is >= 400 and < 500)
                {
                    feature.Logger.LogWarning("Forwarded response failed with {StatusCode}", feature.Context.Response.StatusCode);
                }
                else
                {
                    await next(ctx);
                }
            }
        });
    }

    /// <summary>
    /// Enables use of <see cref="IMemoryResponseBodyFeature"/> to access response.
    /// </summary>
    /// <remarks>
    /// May be called multiple times, but will be inserted only on the first call.
    /// </remarks>
    public static void UseResponseBuffering(this IContextComparerBuilder builder)
    {
        if (builder.Request.HasBeenAdded())
        {
            return;
        }

        builder.Request.UseForwardedContext((ctx, forwarded) =>
        {
            ctx.Response.BufferResponseStreamToMemory();
            forwarded.Context.Response.BufferResponseStreamToMemory();
        });
    }

    private static bool HasBeenAdded(this IApplicationBuilder builder, [CallerMemberName] string key = null!)
    {
        if (builder.Properties.ContainsKey(key))
        {
            return true;
        }

        builder.Properties[key] = true;
        return false;
    }

    private static bool TryGetMemory(HttpContext context, ICheckedForwarderFeature forwarded, out ReadOnlyMemory<byte> localBytes, out ReadOnlyMemory<byte> forwardedBytes)
    {
        if (context.Features.Get<IMemoryResponseBodyFeature>() is { } responseMemory && forwarded.Context.Features.Get<IMemoryResponseBodyFeature>() is { } forwardedMemory)
        {
            forwardedBytes = forwardedMemory.Body;
            localBytes = responseMemory.Body;
            return true;
        }

        forwarded.Logger.LogWarning("Could not retrieve local or forwarded body contents");

        forwardedBytes = default;
        localBytes = default;
        return false;
    }

    /// <summary>
    /// Adds comparison for the response body by comparing bytes
    /// </summary>
    public static void UseBody(this IContextComparerBuilder builder)
    {
        builder.UseResponseBuffering();

        builder.Comparison.UseForwardedContext((context, forwarded) =>
        {
            if (TryGetMemory(context, forwarded, out var localMemory, out var forwardedMemory))
            {
                if (!localMemory.Span.SequenceEqual(forwardedMemory.Span))
                {
                    forwarded.Logger.LogWarning("Forwarded and local contents do not match");
                }
            }
        });
    }

    public static void IgnoreDefault(this HeaderComparisonContext context)
    {
        context.Ignore.Add(HeaderNames.TransferEncoding);
        context.Ignore.Add(HeaderNames.Server);
        context.Ignore.Add(HeaderNames.Date);
    }

    public static void UseHeaders(this IContextComparerBuilder builder)
        => builder.UseHeaders(static _ => { });

    public static void UseHeaders(this IContextComparerBuilder builder, Action<HeaderComparisonContext> configure)
    {
        var headers = new HeaderComparisonContext();
        configure(headers);

        builder.Comparison.UseForwardedContext((context, forwarded) =>
        {
            var visited = new HashSet<string>();

            foreach (var (name, value) in context.Response.Headers)
            {
                if (headers.Ignore.Contains(name))
                {
                    continue;
                }

                visited.Add(name);

                if (forwarded.Context.Response.Headers.TryGetValue(name, out var fromYarp))
                {
                    if (!value.Equals(fromYarp))
                    {
                        forwarded.Logger.LogWarning("Values for header '{HeaderName}' do not match", name);
                    }
                }
                else
                {
                    forwarded.Logger.LogWarning("Local contains '{HeaderName}' while YARP does not", name);
                }
            }

            foreach (var (name, _) in forwarded.Context.Response.Headers)
            {
                if (headers.Ignore.Contains(name))
                {
                    continue;
                }

                if (!visited.Contains(name))
                {
                    forwarded.Logger.LogWarning("YARP result contains '{HeaderName}' while local does not", name);
                }
            }
        });
    }

    public static void UseStatusCode(this IContextComparerBuilder builder)
    {
        builder.Comparison.UseForwardedContext((main, forwarded) =>
        {
            if (main.Response.StatusCode != forwarded.Context.Response.StatusCode)
            {
                forwarded.Logger.LogWarning("Status code for YARP {YarpStatus} is not the same as local {LocalStatus}", main.Response.StatusCode, forwarded.Context.Response.StatusCode);
            }
        });
    }

    private static void UseForwardedContext(this IApplicationBuilder builder, Action<HttpContext, ICheckedForwarderFeature> action)
    {
        builder.Use((ctx, next) =>
        {
            if (ctx.Features.Get<ICheckedForwarderFeature>() is { } feature)
            {
                action(ctx, feature);
            }

            return next(ctx);
        });
    }
}
