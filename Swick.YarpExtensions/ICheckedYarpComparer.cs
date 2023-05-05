using Yarp.ReverseProxy.Forwarder;

namespace Swick.YarpExtensions;

public interface ICheckedYarpComparer
{
    ValueTask CompareAsync(HttpContext local, HttpContext yarp, ForwarderError error);
} 
