namespace Swick.YarpExtensions;

public class HeaderComparisonContext
{
    public ICollection<string> Ignore { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
