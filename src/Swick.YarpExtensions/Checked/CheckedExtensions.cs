using Microsoft.AspNetCore.Builder;
using Swick.YarpExtensions.Checked;
using Swick.YarpExtensions.Comparer;

namespace Swick.YarpExtensions;

public static class CheckedExtensions
{
    public static void UseCheckedForwarder(this IApplicationBuilder app)
    {
        app.UseMiddleware<CheckedForwarderMiddleware>();
    }

    public static T WithCheckedForwarder<T>(this T builder, string destination, Action<IContextComparerBuilder> configure)
        where T : IEndpointConventionBuilder
    {
        ICheckedForwarderMetadata? built = null;

        builder.Add(builder =>
        {
            if (built is { })
            {
                builder.Metadata.Add(built);
            }
            else
            {
                var contextBuilder = new ContextComparerBuilder(destination, builder.ApplicationServices);

                contextBuilder.UseForwarderChecks();

                configure(contextBuilder);

                built = contextBuilder.Build();

                builder.Metadata.Add(built);
            }
        });

        return builder;
    }
}
