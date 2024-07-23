using OldBit.Beep.Extensions;

namespace OldBit.Beep.Readers;

/// <summary>
/// Reads PCM audio data from a stream. It converts the data to 32-bit float values
/// which is the format internally used by the platform's audio system implementation.
/// </summary>
internal class PcmDataReader : IDisposable
{
    private readonly Stream _stream;
    private readonly AudioFormat _format;
    private readonly int _formatByteSize;
    private readonly int _channelCount;
    private bool _disposed;

    internal int Volume { get; set; }

    internal PcmDataReader(Stream input, AudioFormat format, int channelCount)
    {
        _stream = input;
        _format = format;
        _formatByteSize = format.GetByteSize();
        _channelCount = channelCount;
    }

    // TODO: Unify the ReadFrames methods
    internal int ReadFrames(Span<float> destination)
    {
        var source = new byte[destination.Length * _formatByteSize];

        return ReadBuffer(source, destination);
    }

    // TODO: Unify the ReadFrames methods
    internal int ReadFrames(Span<float> destination, int frameCount)
    {
        var source = new byte[frameCount * _formatByteSize * _channelCount];

        return ReadBuffer(source, destination);
    }

    private int ReadBuffer(byte[] source, Span<float> destination)
    {
        var count = _stream.Read(source, 0, source.Length);
        count -= count % _formatByteSize;     // Ensure we have a whole number of samples

        if (count == 0)
        {
            return 0;
        }

        var offset = 0;

        for (var i = 0; i < count; i += _formatByteSize)
        {
            destination[offset++] = _format switch
            {
                AudioFormat.Unsigned8Bit =>
                   VolumeAdjuster.Adjust(source[i], Volume) / 128f - 1,

                AudioFormat.Signed16BitIntegerLittleEndian =>
                    VolumeAdjuster.Adjust(BitConverter.ToInt16(source, i), Volume) / 32768f,

                AudioFormat.Float32BitLittleEndian =>
                    VolumeAdjuster.Adjust(BitConverter.ToSingle(source, i), Volume),

                _ => throw new ArgumentException($"Invalid audio format: {_format}.")
            };
        }

        return offset;
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
