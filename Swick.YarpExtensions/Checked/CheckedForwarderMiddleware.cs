﻿using Microsoft.Extensions.DependencyInjection;
using Swick.YarpExtensions.Checked;
using Swick.YarpExtensions.Features;

namespace Swick.YarpExtensions;

internal sealed class CheckedForwarderMiddleware : IDisposable
{
    private readonly RequestDelegate _next;
    private readonly CheckedForwarder _forwarder;

    public CheckedForwarderMiddleware(RequestDelegate next, IServiceProvider services)
    {
        _next = next;
        _forwarder = ActivatorUtilities.CreateInstance<CheckedForwarder>(services);
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
            await comparisonFeature.CompareAsync();
        }
    }

    public void Dispose()
    {
        _forwarder.Dispose();
    }
}
