using OldBit.Beep.Extensions;
using OldBit.Beep.Filters;

namespace OldBit.Beep.Pcm;

/// <summary>
/// Reads PCM audio data from a stream. It converts the data to 32-bit float values
/// which is the format internally used by the platform's audio system implementation.
/// </summary>
internal sealed class PcmDataReader
{
    private readonly AudioFormat _audioFormat;
    private readonly VolumeFilter _volumeFilter;
    private readonly int _sampleSizeInBytes;
    private byte[] _data = [];
    private int _position;
    private int _count;

    internal PcmDataReader(AudioFormat audioFormat, VolumeFilter volumeFilter)
    {
        _audioFormat = audioFormat;
        _volumeFilter = volumeFilter;
        _sampleSizeInBytes = audioFormat.GetByteSize();
    }

    internal int ReadSamples(Span<float> destination, int sampleCount)
    {
        var data = new ReadOnlySpan<byte>(_data, _position, Math.Min(sampleCount * _sampleSizeInBytes, _count - _position));
        _position += data.Length;

        var count = data.Length;
        count -= count % _sampleSizeInBytes;     // Ensure we have a whole number of samples

        if (count == 0)
        {
            return 0;
        }

        var actualCount = 0;

        for (var i = 0; i < count; i += _sampleSizeInBytes)
        {
            var value = _audioFormat switch
            {
                AudioFormat.Unsigned8Bit => data[i] / 128f - 1,
                AudioFormat.Signed16BitIntegerLittleEndian => BitConverter.ToInt16(data.Slice(i, 2)) / 32768f,
                AudioFormat.Float32BitLittleEndian => BitConverter.ToSingle(data.Slice(i, 4)),
                _ => throw new ArgumentException($"Invalid audio format: {_audioFormat}.")
            };

            value = _volumeFilter.Apply(value);

            destination[actualCount++] = value;
        }

        return actualCount;
    }

    internal void SetData(byte[] data, int count = -1)
    {
        _data = data;
        _count = count == -1 ? _data.Length : count;
        _position = 0;
    }
}
