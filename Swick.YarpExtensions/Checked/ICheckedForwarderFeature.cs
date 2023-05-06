using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions.Checked;

public interface ICheckedForwarderFeature
{
    HttpContext Context { get; }

    ForwarderError Error { get; }

    ValueTask ForwardAsync();

    ValueTask CompareAsync();
}