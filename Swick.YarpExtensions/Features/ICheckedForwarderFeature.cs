using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Features;

public interface ICheckedForwarderFeature
{
    /// <summary>
    /// Gets the <see cref="HttpContext"/> used for forwarding. This is expected to be a read-only view of the request, and a redirected response.
    /// </summary>
    HttpContext Context { get; }

    /// <summary>
    /// Gets the result of forwarding the call. <c>null</c> if forward has not been called.
    /// </summary>
    ForwarderError? Error { get; }

    /// <summary>
    /// Forwards the request and stores any error.
    /// </summary>
    ValueTask ForwardAsync();

    /// <summary>
    /// Compares the result of forwarding. Should forward the request if it has not been forwarded.
    /// </summary>
    ValueTask CompareAsync();
}