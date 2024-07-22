using OldBit.Beep.Extensions;
using OldBit.Beep.Helpers;

namespace OldBit.Beep.Readers;

/// <summary>
/// Reads PCM audio data from a stream. It converts the data to a float values.
/// </summary>
internal class PcmDataReader : IDisposable
{
    private readonly Stream _stream;
    private readonly AudioFormat _format;
    private readonly int _byteSize;
    private readonly int _channelCount;
    private bool _disposed;

    internal int Volume { get; set; }

    internal PcmDataReader(Stream input, AudioFormat format, int channelCount)
    {
        _stream = input;
        _format = format;
        _byteSize = format.GetByteSize();
        _channelCount = channelCount;
    }

    // TODO: Unify the ReadFrames methods
    internal int ReadFrames(Span<float> destination)
    {
        var source = new byte[destination.Length * _byteSize];

        return ReadBuffer(source, destination);
    }

    // TODO: Unify the ReadFrames methods
    internal int ReadFrames(Span<float> destination, int frameCount)
    {
        var source = new byte[frameCount * _byteSize * _channelCount];

        return ReadBuffer(source, destination);
    }

    private int ReadBuffer(byte[] source, Span<float> destination)
    {
        var count = _stream.Read(source, 0, source.Length);

        if (count == 0)
        {
            return 0;
        }

        // Ensure we have a whole number of samples
        count -= count % _byteSize;
       
        var offset = 0;
        
        for (var i = 0; i < count; i += _byteSize)
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
