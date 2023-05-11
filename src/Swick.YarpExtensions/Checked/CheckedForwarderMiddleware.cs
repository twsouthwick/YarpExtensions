using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swick.YarpExtensions.Checked;
using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

internal sealed class CheckedForwarderMiddleware : IDisposable
{
    private readonly RequestDelegate _next;
    private readonly CheckedForwarder _forwarder;
    private readonly ILogger<CheckedForwarderMiddleware> _logger;

    public CheckedForwarderMiddleware(RequestDelegate next, IServiceProvider services, ILogger<CheckedForwarderMiddleware> logger)
    {
        _next = next;
        _forwarder = ActivatorUtilities.CreateInstance<CheckedForwarder>(services);
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<ICheckedForwarderMetadata>() is { } metadata)
        {
            return InvokeCheckedAsync(context, metadata);
        }

        return _next(context);
    }

    private async Task InvokeCheckedAsync(HttpContext context, ICheckedForwarderMetadata metadata)
    {
        var feature = new CheckedForwarderFeature(context, metadata.Comparison, _forwarder, metadata.Destination);
        context.Features.Set<ICheckedForwarderFeature>(feature);

        // Initialize forwarded context if anything is registered - this may remove the feature if it is turned off for the request
        await metadata.Request(context);

        await _next(context);

        // Retrieve it in case someone overwrote it
        if (context.Features.Get<ICheckedForwarderFeature>() is { } comparisonFeature)
        {
            try
            {
                await comparisonFeature.CompareAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unexpected error while comparing forwarded request.");
            }
        }
    }

    public void Dispose()
    {
        _forwarder.Dispose();
    }
}
