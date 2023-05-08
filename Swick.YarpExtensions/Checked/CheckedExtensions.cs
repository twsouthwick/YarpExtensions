﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swick.YarpExtensions.Checked;
using Swick.YarpExtensions.Comparer;
using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

public static class CheckedExtensions
{
    public static void AddCheckedForwarder(this IServiceCollection services)
    {
        services.AddHttpForwarder();
        services.AddSingleton<CheckedForwarder>();
    }

    public static void UseCheckedForwarder(this IApplicationBuilder app)
    {
        var forwarder = app.ApplicationServices.GetRequiredService<CheckedForwarder>();

        app.Use(async (ctx, next) =>
        {
            if (ctx.GetEndpoint()?.Metadata.GetMetadata<ICheckedForwarderMetadata>() is { } metadata)
            {
                // Must be able to replay request
                ctx.Request.EnableBuffering();

                // Run pipeline of any updates to the main context if needed.
                await metadata.MainContext(ctx);

                var feature = new CheckedForwarderFeature(ctx, metadata.Comparison, forwarder, metadata.Destination);
                ctx.Features.Set<ICheckedForwarderFeature>(feature);

                // Initialize forwarded context if anything is registered
                await metadata.ForwardedContext(feature.Context);

                await next(ctx);

                // Retrieve it in case someone overwrote it
                if (ctx.Features.Get<ICheckedForwarderFeature>() is { } comparisonFeature)
                {
                    await comparisonFeature.CompareAsync();
                }
            }
            else
            {
                await next(ctx);
            }
        });
    }

    public static IEndpointConventionBuilder WithCheckedForwarder(this IEndpointConventionBuilder builder, string destination, Action<IContextComparerBuilder> comparer)
    {
        builder.Add(builder =>
        {
            var contextBuilder = new ContextComparerBuilder(destination, builder.ApplicationServices);

            comparer(contextBuilder);

            builder.Metadata.Add(contextBuilder.Build());
        });

        return builder;
    }
}
