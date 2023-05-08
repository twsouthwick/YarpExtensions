using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Swick.YarpExtensions.Comparer;
using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

public static class ContextComparerExtensions
{
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

    public static void CompareHeaders(this IContextComparerBuilder builder, params string[] ignoredHeaders)
    {
        var ignore = new HashSet<string>(ignoredHeaders, StringComparer.OrdinalIgnoreCase);

        builder.Comparison.UseForwardedContext((context, forwarded) =>
        {
            var visited = new HashSet<string>();

            foreach (var (name, value) in context.Response.Headers)
            {
                if (ignoredHeaders.Contains(name))
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
                if (ignore.Contains(name))
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
