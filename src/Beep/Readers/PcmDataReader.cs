using OldBit.Beep.Extensions;
using OldBit.Beep.Filters;

namespace OldBit.Beep.Readers;

/// <summary>
/// Reads PCM audio data from a stream. It converts the data to 32-bit float values
/// which is the format internally used by the platform's audio system implementation.
/// </summary>
internal sealed class PcmDataReader : IDisposable
{
    private readonly Stream _stream;
    private readonly AudioFormat _audioFormat;
    private readonly int _sampleSizeInBytes;
    private bool _disposed;

    internal VolumeFilter VolumeFilter { get; } = new();

    internal PcmDataReader(Stream input, AudioFormat audioFormat)
    {
        _stream = input;
        _audioFormat = audioFormat;
        _sampleSizeInBytes = audioFormat.GetByteSize();
    }

    internal int ReadFrames(Span<float> destination, int frameCount)
    {
        var buffer = new byte[frameCount * _sampleSizeInBytes];

        var count = _stream.Read(buffer, 0, buffer.Length);
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
                    VolumeFilter.Apply(buffer[i]) / 128f - 1,

                AudioFormat.Signed16BitIntegerLittleEndian =>
                    VolumeFilter.Apply(BitConverter.ToInt16(buffer, i)) / 32768f,

                AudioFormat.Float32BitLittleEndian =>
                    VolumeFilter.Apply(BitConverter.ToSingle(buffer, i)),

                _ => throw new ArgumentException($"Invalid audio format: {_audioFormat}.")
            };
        }

        return offset;
    }

    internal void Close() => Dispose();

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
        GC.SuppressFinalize(this);
    }
}
