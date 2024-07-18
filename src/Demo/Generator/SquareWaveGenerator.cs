using OldBit.Beep;
using OldBit.Beep.Extensions;

namespace Demo.Generator;

public class SquareWaveGenerator(AudioFormat format, int sampleRate = 44100, int channelCount = 2) : IWaveGenerator
{
    public IEnumerable<byte> Generate(float frequency, TimeSpan duration)
    {
        var sampleLength = (int)(sampleRate / frequency);

        var byteSize = format.GetByteSize() * channelCount;
        var bufferSize = CalculateBufferSize(duration);

        for (var i = 0; i < bufferSize / byteSize; i++)
        {
            double sampleValue;
            var highAmplitude = i % sampleLength < sampleLength / 2;

            switch (format)
            {
                case AudioFormat.Unsigned8Bit:
                    sampleValue = highAmplitude ? 0xFF : 0x00;
                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        yield return (byte)sampleValue;
                    }
                    break;

                case AudioFormat.Signed16BitIntegerLittleEndian:
                    sampleValue = highAmplitude ? 32767 : -32767;
                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        yield return (byte)sampleValue;
                        yield return (byte)((int)sampleValue >> 8);
                    }
                    break;

                case AudioFormat.Float32BitLittleEndian:
                    sampleValue = highAmplitude ? 1f : -1f;
                    var bytes = BitConverter.GetBytes((float)sampleValue);

                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        yield return bytes[0];
                        yield return bytes[1];
                        yield return bytes[2];
                        yield return bytes[3];
                    }
                    break;
            }
        }
    }

    private int CalculateBufferSize(TimeSpan duration)
    {
        var size = format.GetByteSize() * channelCount * sampleRate * duration.TotalSeconds;

        return (int)Math.Ceiling(size / 4) * 4;
    }
}