namespace Swick.YarpExtensions.Features;

public interface IMemoryResponseBodyFeature
{
    ReadOnlyMemory<byte> Body { get; }
}