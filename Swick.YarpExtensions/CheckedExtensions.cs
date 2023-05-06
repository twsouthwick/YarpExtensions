using Microsoft.AspNetCore.Builder;
using Swick.YarpExtensions.Checked;

namespace Swick.YarpExtensions;

public static class CheckedExtensions
{
    public static void UseCheckedForwarder(this IApplicationBuilder app)
    {
        app.UseWhen(context => context.GetCheckedMetadata() is not null, app =>
        {
            app.UseMiddleware<BufferResponseStreamForReplayMiddleware>();
            app.Use(async (context, next) =>
            {
                await next(context);

                if (context.Features.Get<ICheckedForwarderFeature>() is { } feature)
                {
                    await feature.CompareAsync();
                }
            });
            app.UseMiddleware<CheckedForwarderMiddleware>();
        });
    }

    public static IEndpointConventionBuilder WithCheckedYarp(this IEndpointConventionBuilder builder, string destination)
        => builder.WithMetadata(new CheckedYarpMetadata(destination));

    internal static CheckedYarpMetadata? GetCheckedMetadata(this HttpContext context) => context.GetEndpoint()?.Metadata.GetMetadata<CheckedYarpMetadata>();
}
