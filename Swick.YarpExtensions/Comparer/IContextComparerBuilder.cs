using Microsoft.AspNetCore.Builder;

namespace Swick.YarpExtensions.Comparer;

public interface IContextComparerBuilder
{
    IApplicationBuilder MainContext { get; }

    IApplicationBuilder ForwardedContext { get; }

    IApplicationBuilder Comparison { get; }
}
