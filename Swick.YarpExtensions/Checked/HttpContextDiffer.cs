using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal class HttpContextDiffer
{
    private readonly IOptions<CheckedYarpOptions> _options;
    private readonly ILogger<HttpContextDiffer> _logger;

    public HttpContextDiffer(IOptions<CheckedYarpOptions> options, ILogger<HttpContextDiffer> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async ValueTask CompareAsync(HttpContext local, HttpContext yarp, ForwarderError error)
    {
        using (_logger.BeginScope("Comparing forwarded YARP for {Path}", local.Request.Path))
        {
            if (error != ForwarderError.None)
            {
                _logger.LogWarning("Unexpected error forwarding to YARP: {Error}", error);
            }
            else
            {
                CompareStatus(local, yarp);
                CompareHeaders(local, yarp);
                await CompareBody(local, yarp);
            }
        }
    }

    private void CompareStatus(HttpContext local, HttpContext yarp)
    {
        if (local.Response.StatusCode != yarp.Response.StatusCode)
        {
            _logger.LogWarning("Status code for YARP {YarpStatus} is not the same as local {LocalStatus}", local.Response.StatusCode, yarp.Response.StatusCode);
        }
    }

    private void CompareHeaders(HttpContext local, HttpContext yarp)
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
                    _logger.LogWarning("Values for header '{HeaderName}' do not match", name);
                }
            }
            else
            {
                _logger.LogWarning("Local contains '{HeaderName}' while YARP does not", name);
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
                _logger.LogWarning("YARP result contains '{HeaderName}' while local does not", name);
            }
        }
    }

    private async ValueTask CompareBody(HttpContext local, HttpContext yarp)
    {
        var localBody = local.Response.Body;
        var yarpBody = yarp.Response.Body;

        if (localBody.Length != yarpBody.Length)
        {
            _logger.LogWarning("YARP and local body do not match length");
        }
        else if (!await StreamEquals(localBody, yarpBody, local.RequestAborted))
        {
            _logger.LogWarning("YARP and local contents do not match length");
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
