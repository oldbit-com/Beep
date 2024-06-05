namespace OldBit.Beep.Readers;

/// <summary>
/// The ByteStream class allows reading enumeration of bytes as a stream.
/// </summary>
internal class ByteStream(IEnumerable<byte> data) : Stream
{
    private readonly IEnumerator<byte> _input = data.GetEnumerator();

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var position = 0;

        for (; position < count && _input.MoveNext(); position++)
        {
            buffer[position + offset] = _input.Current;
        }

        return position;
    }

    public override long Seek(long offset, SeekOrigin origin)=> throw new NotSupportedException();

    public override void SetLength(long value)=> throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _input.Dispose();
        }
    }
}