using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Swick.YarpExtensions.Comparer;
using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

public static class ContextComparerExtensions
{
    public static void IsEnabledWhen(this IContextComparerBuilder builder, Func<HttpContext, ValueTask<bool>> predicate)
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

    public static void BodyMustBeEqual(this IContextComparerBuilder builder)
    {
        builder.Request.UseForwardedContext((ctx, forwarded) =>
        {
            ctx.Response.BufferResponseStreamToMemory();
            forwarded.Context.Response.BufferResponseStreamToMemory();
        });

        builder.Comparison.UseForwardedContext((context, forwarded) =>
        {
            if (context.Features.Get<IMemoryResponseBodyFeature>() is { } responseMemory && forwarded.Context.Features.Get<IMemoryResponseBodyFeature>() is { } forwardedMemory)
            {
                if (responseMemory.Body.Length != forwardedMemory.Body.Length)
                {
                    forwarded.Logger.LogWarning("YARP and local body do not match length");
                }
                else
                {
                    if (!responseMemory.Body.Span.SequenceEqual(forwardedMemory.Body.Span))
                    {
                        forwarded.Logger.LogWarning("YARP and local contents do not match length");
                    }
                }
            }
            else
            {
                forwarded.Logger.LogWarning("Could not compare body contents");
            }
        });
    }

    public static void IgnoreHeaders(this IContextComparerBuilder builder, params string[] headers)
    {
        var set = builder.GetIgnoredHeaders();

        set.UnionWith(headers);

        builder.CompareHeaders();
    }

    public static void IgnoreDefaultHeaders(this IContextComparerBuilder builder)
        => builder.IgnoreHeaders(
            HeaderNames.TransferEncoding,
            HeaderNames.Server,
            HeaderNames.Date);

    private static HashSet<string> GetIgnoredHeaders(this IContextComparerBuilder builder)
    {
        const string IgnoredKey = "IgnoredHeaders";

        if (builder.Comparison.Properties.TryGetValue(IgnoredKey, out var ignored) && ignored is HashSet<string> set)
        {
            return set;
        }

        set = new();
        builder.Comparison.Properties[IgnoredKey] = set;
        return set;
    }

    public static void CompareHeaders(this IContextComparerBuilder builder)
    {
        const string CompareAddedKey = "compareAdded";

        if (builder.Comparison.Properties.ContainsKey(CompareAddedKey))
        {
            return;
        }

        builder.Comparison.Properties[CompareAddedKey] = true;
        var ignored = builder.GetIgnoredHeaders();

        builder.Comparison.UseForwardedContext((context, forwarded) =>
        {
            var visited = new HashSet<string>();

            foreach (var (name, value) in context.Response.Headers)
            {
                if (ignored.Contains(name))
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
                if (ignored.Contains(name))
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

    public static void CompareStatusCodes(this IContextComparerBuilder builder)
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
        builder.Use(async (ctx, next) =>
        {
            await next(ctx);

            if (ctx.Features.Get<ICheckedForwarderFeature>() is { } feature)
            {
                action(ctx, feature);
            }
        });
    }
}
