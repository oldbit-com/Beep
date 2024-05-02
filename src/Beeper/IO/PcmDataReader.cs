using OldBit.Beeper.Helpers;

namespace OldBit.Beeper.IO;

internal class PcmDataReader : IDisposable
{
    private readonly Stream _stream;
    private readonly AudioFormat _format;
    private bool _disposed;

    internal PcmDataReader(Stream input, AudioFormat format)
    {
        _stream = input;
        _format = format;
    }

    internal int Read(Span<float> buffer)
    {
        var byteSize = AudioFormatHelper.GetByteSize(_format);
        var byteBuffer = new byte[buffer.Length * byteSize];
        var bytesRead = _stream.Read(byteBuffer, 0, buffer.Length * byteSize);

        bytesRead -= bytesRead % byteSize; // Ensure we have a whole number of samples

        if (bytesRead == 0)
        {
            return 0;
        }

        var offset = 0;
        for (var i = 0; i < bytesRead; i += byteSize)
        {
            buffer[offset++] = _format switch
            {
                AudioFormat.Unsigned8Bit =>
                    byteBuffer[i] / 128f - 1,

                AudioFormat.Signed16BitIntegerLittleEndian =>
                    BitConverter.ToInt16(byteBuffer, i) / 32768f,

                AudioFormat.Float32BitLittleEndian =>
                    BitConverter.ToSingle(byteBuffer, i),

                _ => throw new ArgumentException($"Invalid audio format: {_format}.")
            };
        }

        return offset;
    }

    internal void Close()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _stream.Close();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
    }
}