using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

internal static class ResponseStreamExtensions
{
    public static void BufferResponseStreamToMemory(this HttpResponse response)
    {
        if (response.HttpContext.Features.Get<IMemoryResponseBodyFeature>() is null)
        {
            var memoryResponse = new MemoryBackedPassThroughStream(response.Body);

            response.Body = memoryResponse;
            response.RegisterForDisposeAsync(memoryResponse);

            response.HttpContext.Features.Set<IMemoryResponseBodyFeature>(memoryResponse);
        }
    }
}