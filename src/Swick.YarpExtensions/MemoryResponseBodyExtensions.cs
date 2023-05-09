using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

public static class MemoryResponseBodyExtensions
{
    /// <summary>
    /// Gets a unique readonly stream of the response body.
    /// </summary>
    public static Stream GetStream(this IMemoryResponseBodyFeature feature) => new ReadOnlyMemoryStream(feature.Body);
}
