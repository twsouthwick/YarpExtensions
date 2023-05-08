using Microsoft.AspNetCore.Builder;

namespace Swick.YarpExtensions.Comparer;

public interface IContextComparerBuilder
{
    IApplicationBuilder Request { get; }

    IApplicationBuilder Comparison { get; }
}
