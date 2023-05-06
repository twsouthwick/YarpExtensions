namespace Swick.YarpExtensions;

public class CheckedForwarderOptions
{
    public ICollection<string> IgnoredHeaders { get; } = new HashSet<string>();
}
