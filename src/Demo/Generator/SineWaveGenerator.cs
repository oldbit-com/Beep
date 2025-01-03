using OldBit.Beep;
using OldBit.Beep.Extensions;

namespace Demo.Generator;

public class SineWaveGenerator(AudioFormat format, int sampleRate = 44100, int channelCount = 2) : IWaveGenerator
{
    public IEnumerable<byte> Generate(float frequency, TimeSpan duration)
    {
        var byteSize = format.GetByteSize() * channelCount;
        var bufferSize = CalculateBufferSize(duration);

        for (var i = 0; i < bufferSize / byteSize; i++)
        {
            var angle = 2 * Math.PI * frequency * i / sampleRate;

            double sampleValue;

            switch (format)
            {
                case AudioFormat.Unsigned8Bit:
                    sampleValue = GetSampleValue(angle, amplitude: 127) + 128;

                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        yield return (byte)sampleValue;
                    }

                    break;

                case AudioFormat.Signed16BitIntegerLittleEndian:
                    sampleValue = GetSampleValue(angle, amplitude: 32767);

                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        yield return (byte)sampleValue;
                        yield return (byte)((short)sampleValue >> 8);
                    }

                    break;

                case AudioFormat.Float32BitLittleEndian:
                    sampleValue = GetSampleValue(angle, amplitude: 1f);

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

    private static double GetSampleValue(double angle, double amplitude) => amplitude * Math.Sin(angle);

    private int CalculateBufferSize(TimeSpan duration)
    {
        var size = format.GetByteSize() * channelCount * sampleRate * duration.TotalSeconds;

        return (int)Math.Ceiling(size / 4) * 4;
    }
}