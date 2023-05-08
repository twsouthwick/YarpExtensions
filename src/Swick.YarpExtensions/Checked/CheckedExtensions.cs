using Microsoft.AspNetCore.Builder;
using Swick.YarpExtensions.Comparer;

namespace Swick.YarpExtensions;

public static class CheckedExtensions
{
    public static void UseCheckedForwarder(this IApplicationBuilder app)
    {
        app.UseMiddleware<CheckedForwarderMiddleware>();
    }

    public static T WithCheckedForwarder<T>(this T builder, string destination, Action<IContextComparerBuilder> comparer)
        where T : IEndpointConventionBuilder
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
