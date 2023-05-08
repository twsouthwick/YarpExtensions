using Swick.YarpExtensions.Features;
using System.Buffers;

namespace Swick.YarpExtensions;

internal class MemoryBackedPassThroughStream : Stream, IMemoryResponseBodyFeature
{
    private static readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

    private readonly Stream _inner;
    private byte[] _array;
    private int _length;

    public MemoryBackedPassThroughStream(Stream inner)
    {
        _inner = inner;
        _array = Array.Empty<byte>();
    }

    private void EnsureSize(int toWrite)
    {
        if (_array.Length < _length + toWrite)
        {
            Resize();
        }
    }

    private void Resize()
    {
        var size = _array.Length * 2;

        if (size == 0)
        {
            size = 1024;
        }

        var newArray = _pool.Rent(size);
        Array.Clear(newArray);
        Array.Copy(_array, newArray, _length);

        var current = _array;
        _array = newArray;

        _pool.Return(current);
    }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => _inner.CanSeek;

    public override bool CanWrite => _inner.CanWrite;

    public override long Length => _length;

    public ReadOnlyMemory<byte> Memory => _array.AsMemory(0, _length);

    ReadOnlyMemory<byte> IMemoryResponseBodyFeature.Body => Memory;

    public override void Flush() => _inner.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

    public override void Close()
    {
        ClearArray();
    }

    private void ClearArray()
    {
        if (_array.Length > 0)
        {
            _pool.Return(_array);
            _array = Array.Empty<byte>();
            _length = 0;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ClearArray();
    }

    public override ValueTask DisposeAsync()
    {
        ClearArray();
        return _inner.DisposeAsync();
    }

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override long Seek(long offset, SeekOrigin origin)
        => _inner.Seek(offset, origin);

    public override void SetLength(long value)
        => _inner.SetLength(value);

    public override bool CanTimeout => _inner.CanTimeout;

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteInternal(buffer.AsSpan(offset, count));
        _inner.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        WriteInternal(buffer);
        _inner.Write(buffer);
    }

    public override int WriteTimeout
    {
        get => _inner.WriteTimeout;
        set => _inner.WriteTimeout = value;
    }

    private void WriteInternal(ReadOnlySpan<byte> buffer)
    {
        EnsureSize(buffer.Length);
        buffer.CopyTo(_array.AsSpan(_length));
        _length += buffer.Length;
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        WriteInternal(buffer.AsSpan(offset, count));
        return _inner.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        WriteInternal(buffer.Span);
        return _inner.WriteAsync(buffer, cancellationToken);
    }

    public override void WriteByte(byte value)
    {
        Span<byte> buffer = stackalloc byte[1];
        buffer[0] = value;
        WriteInternal(buffer);
        _inner.WriteByte(value);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => throw new NotImplementedException();

    public override void EndWrite(IAsyncResult asyncResult)
        => throw new NotImplementedException();

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => _inner.CopyToAsync(destination, bufferSize, cancellationToken);

    public override void CopyTo(Stream destination, int bufferSize)
        => _inner.CopyTo(destination, bufferSize);

    public override int Read(byte[] buffer, int offset, int count)
        => _inner.Read(buffer, offset, count);

    public override int Read(Span<byte> buffer)
        => _inner.Read(buffer);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _inner.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => _inner.ReadAsync(buffer, cancellationToken);

    public override int ReadByte() => _inner.ReadByte();

    public override int ReadTimeout
    {
        get => _inner.ReadTimeout;
        set => _inner.ReadTimeout = value;
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => _inner.BeginRead(buffer, offset, count, callback, state);

    public override int EndRead(IAsyncResult asyncResult)
        => _inner.EndRead(asyncResult);
}
