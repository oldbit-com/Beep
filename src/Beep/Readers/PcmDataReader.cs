using OldBit.Beep.Extensions;

namespace OldBit.Beep.Readers;

/// <summary>
/// Reads PCM audio data from a stream. It converts the data to 32-bit float values
/// which is the format internally used by the platform's audio system implementation.
/// </summary>
internal class PcmDataReader : IDisposable
{
    private readonly Stream _stream;
    private readonly AudioFormat _audioFormat;
    private readonly int _sampleSizeInBytes;
    private bool _disposed;

    internal int Volume { get; set; }

    internal PcmDataReader(Stream input, AudioFormat audioFormat)
    {
        _stream = input;
        _audioFormat = audioFormat;
        _sampleSizeInBytes = audioFormat.GetByteSize();
    }

    internal int ReadFrames(Span<float> destination, int frameCount)
    {
        var source = new byte[frameCount * _sampleSizeInBytes];

        return ReadBuffer(source, destination);
    }

    private int ReadBuffer(byte[] source, Span<float> destination)
    {
        var count = _stream.Read(source, 0, source.Length);
        count -= count % _sampleSizeInBytes;     // Ensure we have a whole number of samples

        if (count == 0)
        {
            return 0;
        }

        var offset = 0;

        for (var i = 0; i < count; i += _sampleSizeInBytes)
        {
            destination[offset++] = _audioFormat switch
            {
                AudioFormat.Unsigned8Bit =>
                   VolumeAdjuster.Adjust(source[i], Volume) / 128f - 1,

                AudioFormat.Signed16BitIntegerLittleEndian =>
                    VolumeAdjuster.Adjust(BitConverter.ToInt16(source, i), Volume) / 32768f,

                AudioFormat.Float32BitLittleEndian =>
                    VolumeAdjuster.Adjust(BitConverter.ToSingle(source, i), Volume),

                _ => throw new ArgumentException($"Invalid audio format: {_audioFormat}.")
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
