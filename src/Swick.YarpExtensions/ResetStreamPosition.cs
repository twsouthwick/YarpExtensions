namespace Swick.YarpExtensions;

internal readonly struct ResetStreamPosition : IDisposable
{
    private readonly Stream _stream;
    private readonly long _position;

    public ResetStreamPosition(Stream stream)
    {
        _stream = stream;
        _position = stream.Position;
    }

    public ResetStreamPosition(Stream stream, int startingPosition)
        : this(stream)
    {
        stream.Position = startingPosition;
    }

    public void Dispose()
    {
        _stream.Position = _position;
    }
}

