using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swick.YarpExtensions.Checked;
using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

public static class CheckedExtensions
{
    public static void AddCheckedForwarder(this IServiceCollection services)
    {
        services.AddHttpForwarder();
        services.AddSingleton<HttpContextDiffer>();
        services.AddSingleton<CheckedForwarder>();
    }

    public static void AddCheckedForwarder(this IServiceCollection services, Action<CheckedForwarderOptions> configure)
    {
        services.AddCheckedForwarder();
        services.AddOptions<CheckedForwarderOptions>()
            .Configure(configure);
    }

    public static void UseCheckedForwarder(this IApplicationBuilder app)
    {
        var differ = app.ApplicationServices.GetRequiredService<HttpContextDiffer>();
        var forwarder = app.ApplicationServices.GetRequiredService<CheckedForwarder>();

        app.UseWhen(
            context =>
            {
                if (context.GetCheckedMetadata() is { } metadata)
                {
                    context.Features.Set<ICheckedForwarderFeature>(new CheckedForwarderFeature(context, differ, forwarder, metadata.Destination));
                    return true;
                }

                return false;
            },
            app =>
            {
                app.UseMiddleware<BufferResponseStreamForReplayMiddleware>();
                app.Use(async (ctx, next) =>
                {
                    await next(ctx);

                    if (ctx.Features.Get<ICheckedForwarderFeature>() is { } feature)
                    {
                        await feature.CompareAsync();
                    }
                });
            });
    }

    public static IEndpointConventionBuilder WithCheckedYarp(this IEndpointConventionBuilder builder, string destination)
        => builder.WithMetadata(new CheckedForwarderMetadata(destination));

    private static CheckedForwarderMetadata? GetCheckedMetadata(this HttpContext context) => context.GetEndpoint()?.Metadata.GetMetadata<CheckedForwarderMetadata>();
}
