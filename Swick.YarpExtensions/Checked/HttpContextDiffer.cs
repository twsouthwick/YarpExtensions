﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal class HttpContextDiffer
{
    private readonly IOptions<CheckedYarpOptions> _options;

    public HttpContextDiffer(IOptions<CheckedYarpOptions> options)
    {
        _options = options;
    }

    public async ValueTask CompareAsync(HttpContext local, HttpContext yarp, ForwarderError error)
    {
        var logger = local.RequestServices.GetRequiredService<ILogger<HttpContextDiffer>>();

        using (logger.BeginScope("Comparing forwarded YARP for {Path}", local.Request.Path))
        {
            if (error != ForwarderError.None)
            {
                logger.LogWarning("Unexpected error forwarding to YARP: {Error}", error);
            }
            else
            {
                CompareStatus(logger, local, yarp);
                CompareHeaders(logger, local, yarp);
                await CompareBody(logger, local, yarp);
            }
        }
    }

    private static void CompareStatus(ILogger logger, HttpContext local, HttpContext yarp)
    {
        if (local.Response.StatusCode != yarp.Response.StatusCode)
        {
            logger.LogWarning("Status code for YARP {YarpStatus} is not the same as local {LocalStatus}", local.Response.StatusCode, yarp.Response.StatusCode);
        }
    }

    private void CompareHeaders(ILogger logger, HttpContext local, HttpContext yarp)
    {
        var visited = new HashSet<string>();

        foreach (var (name, value) in local.Response.Headers)
        {
            if (_options.Value.IgnoredHeaders.Contains(name))
            {
                continue;
            }

            visited.Add(name);

            if (yarp.Response.Headers.TryGetValue(name, out var fromYarp))
            {
                if (!value.Equals(fromYarp))
                {
                    logger.LogWarning("Values for header '{HeaderName}' do not match", name);
                }
            }
            else
            {
                logger.LogWarning("Local contains '{HeaderName}' while YARP does not", name);
            }
        }

        foreach (var (name, _) in yarp.Response.Headers)
        {
            if (_options.Value.IgnoredHeaders.Contains(name))
            {
                continue;
            }

            if (!visited.Contains(name))
            {
                logger.LogWarning("YARP result contains '{HeaderName}' while local does not", name);
            }
        }
    }

    private static async ValueTask CompareBody(ILogger logger, HttpContext local, HttpContext yarp)
    {
        var localBody = local.Response.Body;
        var yarpBody = yarp.Response.Body;

        if (localBody.Length != yarpBody.Length)
        {
            logger.LogWarning("YARP and local body do not match length");
        }
        else if (!await StreamEquals(localBody, yarpBody, local.RequestAborted))
        {
            logger.LogWarning("YARP and local contents do not match length");
        }
    }

    private static async ValueTask<bool> StreamEquals(Stream stream1, Stream stream2, CancellationToken token)
    {
        using (new ResetStreamPosition(stream1, 0))
        using (new ResetStreamPosition(stream2, 0))
        {
            var length = (int)stream1.Length;
            var bytes1 = ArrayPool<byte>.Shared.Rent(length);
            var bytes2 = ArrayPool<byte>.Shared.Rent(length);
            var memory1 = new Memory<byte>(bytes1, 0, length);
            var memory2 = new Memory<byte>(bytes2, 0, length);

            try
            {
                await Task.WhenAll(stream1.ReadExactlyAsync(memory1, token).AsTask(), stream2.ReadExactlyAsync(memory2, token).AsTask());

                return memory1.Span.SequenceEqual(memory2.Span);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes1);
                ArrayPool<byte>.Shared.Return(bytes2);
            }
        }
    }
}
