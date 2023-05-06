using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swick.YarpExtensions.Features;
using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

internal class HttpContextDiffer
{
    private readonly IOptions<CheckedForwarderOptions> _options;
    private readonly ILogger<HttpContextDiffer> _logger;

    public HttpContextDiffer(IOptions<CheckedForwarderOptions> options, ILogger<HttpContextDiffer> logger)
    {
        _options = options;
        _logger = logger;
    }

    public ValueTask CompareAsync(HttpContext local, HttpContext yarp, ForwarderError error)
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
                CompareBody(local, yarp);
            }
        }

        return ValueTask.CompletedTask;
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

    private void CompareBody(HttpContext local, HttpContext yarp)
    {
        if (local.Features.Get<IMemoryResponseBodyFeature>() is { } localMemory && yarp.Features.Get<IMemoryResponseBodyFeature>() is { } yarpMemory)
        {
            if (localMemory.Body.Length != yarpMemory.Body.Length)
            {
                _logger.LogWarning("YARP and local body do not match length");
            }
            else
            {
                if (!localMemory.Body.Span.SequenceEqual(yarpMemory.Body.Span))
                {
                    _logger.LogWarning("YARP and local contents do not match length");
                }
            }
        }
        else
        {
            _logger.LogWarning("Could not compare body contents");
        }
    }
}