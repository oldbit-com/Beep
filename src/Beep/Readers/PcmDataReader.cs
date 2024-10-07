using OldBit.Beep.Extensions;
using OldBit.Beep.Filters;

namespace OldBit.Beep.Readers;

/// <summary>
/// Reads PCM audio data from a stream. It converts the data to 32-bit float values
/// which is the format internally used by the platform's audio system implementation.
/// </summary>
internal sealed class PcmDataReader
{
    private readonly IEnumerable<byte> _data;
    private readonly AudioFormat _audioFormat;
    private readonly int _sampleSizeInBytes;
    private int _position;

    internal List<IAudioFilter> Filters { get; init; } = null!;

    internal PcmDataReader(IEnumerable<byte> data, AudioFormat audioFormat)
    {
        _data = data;
        _audioFormat = audioFormat;
        _sampleSizeInBytes = audioFormat.GetByteSize();
    }

    internal int ReadFrames(Span<float> destination, int frameCount)
    {
        var data = _data.Skip(_position).Take(frameCount * _sampleSizeInBytes).ToArray();
        _position += data.Length;

        var count = data.Length;
        count -= count % _sampleSizeInBytes;     // Ensure we have a whole number of samples

        if (count == 0)
        {
            return 0;
        }

        var offset = 0;

        for (var i = 0; i < count; i += _sampleSizeInBytes)
        {
            var value = _audioFormat switch
            {
                AudioFormat.Unsigned8Bit => data[i] / 128f - 1,
                AudioFormat.Signed16BitIntegerLittleEndian => BitConverter.ToInt16(data, i) / 32768f,
                AudioFormat.Float32BitLittleEndian =>BitConverter.ToSingle(data, i),
                _ => throw new ArgumentException($"Invalid audio format: {_audioFormat}.")
            };

            foreach (var filter in Filters)
            {
                value = filter.Apply(value);
            }

            destination[offset++] = value;
        }

        return offset;
    }
}
