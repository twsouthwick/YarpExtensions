namespace Swick.YarpExtensions;

internal sealed class ReadOnlyMemoryStream : Stream
{
    private readonly ReadOnlyMemory<byte> _memory;

    public ReadOnlyMemoryStream(ReadOnlyMemory<byte> memory)
    {
        _memory = memory;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _memory.Length;

    public override long Position { get; set; }

    public override void Flush() => throw new NotImplementedException();

    public override int Read(byte[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        var length = Position - Math.Min(Position + buffer.Length, _memory.Length);

        _memory.Span.Slice((int)Position, (int)length).CopyTo(buffer);

        return (int)length;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Task.FromResult(Read(buffer, offset, count));

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Read(buffer.Span));

    public override int ReadByte() => _memory.Span[(int)Position++];

    public override long Seek(long offset, SeekOrigin origin)
        => Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length - offset,
            _ => throw new NotImplementedException(),
        };

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
}
