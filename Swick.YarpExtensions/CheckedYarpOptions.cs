namespace Swick.YarpExtensions;

public class CheckedYarpOptions
{
    public ICollection<string> IgnoredHeaders { get; } = new HashSet<string>();
}
