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
    private IEnumerable<byte> _data = [];
    private int _position;

    internal PcmDataReader(AudioFormat audioFormat, VolumeFilter volumeFilter)
    {
        _audioFormat = audioFormat;
        _volumeFilter = volumeFilter;
        _sampleSizeInBytes = audioFormat.GetByteSize();
    }

    internal int ReadSamples(Span<float> destination, int sampleCount)
    {
        var data = _data.Skip(_position).Take(sampleCount * _sampleSizeInBytes).ToArray();
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
                AudioFormat.Signed16BitIntegerLittleEndian => BitConverter.ToInt16(data, i) / 32768f,
                AudioFormat.Float32BitLittleEndian => BitConverter.ToSingle(data, i),
                _ => throw new ArgumentException($"Invalid audio format: {_audioFormat}.")
            };

            value = _volumeFilter.Apply(value);

            destination[actualCount++] = value;
        }

        return actualCount;
    }

    internal IEnumerable<byte> Data
    {
        set
        {
            _data = value;
            _position = 0;
        }
    }
}
