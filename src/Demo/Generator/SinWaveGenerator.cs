using OldBit.Beeper;
using OldBit.Beeper.Helpers;

namespace Demo.Generator;

/// <summary>
/// Generates a simple sine wave.
/// </summary>
/// <param name="format">The audio format.</param>
/// <param name="sampleRate">The sample rate. Typically 44100, 48000.</param>
/// <param name="channelCount">The number of channels.</param>
public class SinWaveGenerator(AudioFormat format, int sampleRate = 44100, int channelCount = 2)
{
    private int _sampleNumber;

    public IEnumerable<byte> Generate(float frequency, TimeSpan duration)
    {
        var sampleLength = sampleRate / frequency;

        var byteSize = AudioFormatHelper.GetByteSize(format) * channelCount;
        var bufferSize = CalculateBufferSize(duration);

        for (var i = 0; i < bufferSize / byteSize; i++)
        {
            var multiplier  = 2 * Math.PI * _sampleNumber / sampleLength;
            double sampleValue;

            switch (format)
            {
                case AudioFormat.Unsigned8Bit:
                    sampleValue = Math.Sin(multiplier) * 0.3 * 127;

                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        yield return (byte)(sampleValue + 128);
                    }

                    break;

                case AudioFormat.Signed16BitIntegerLittleEndian:
                    sampleValue = Math.Sin(multiplier) * 0.3 * 32767;

                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        yield return (byte)sampleValue;
                        yield return (byte)((int)sampleValue >> 8);
                    }

                    break;

                case AudioFormat.Float32BitLittleEndian:
                    sampleValue = Math.Sin(multiplier) * 0.3;
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

            _sampleNumber += 1;
        }
    }

    /// <summary>
    /// Calculates the buffer size needed for the specified duration. It is a multiple of 4.
    /// </summary>
    /// <param name="duration">The duration for which the buffer size is to be calculated.</param>
    /// <returns>The calculated buffer size.</returns>
    private int CalculateBufferSize(TimeSpan duration)
    {
        var size = AudioFormatHelper.GetByteSize(format) * channelCount * sampleRate * duration.TotalSeconds;

        return (int)Math.Ceiling(size / 4) * 4;
    }
}