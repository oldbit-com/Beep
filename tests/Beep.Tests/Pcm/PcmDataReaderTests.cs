using FluentAssertions;
using OldBit.Beep.Filters;
using OldBit.Beep.Pcm;

namespace OldBit.Beep.Tests.Pcm;

public class PcmDataReaderTests
{
    [Fact]
    public void Unsigned8Bit_ShouldReturnFrames()
    {
        var reader = new PcmDataReader(AudioFormat.Unsigned8Bit, new VolumeFilter(100))
        {
            Data = new byte[] { 1, 50, 100, 255 }
        };

        var buffer = new float[10];
        var count = reader.ReadFrames(buffer, buffer.Length);

        count.Should().Be(4);
        buffer.Should().BeEquivalentTo([-0.9921875f, -0.609375f, -0.21875f, 0.9921875f, 0, 0, 0, 0, 0, 0]);
    }

    [Fact]
    public void Signed16BitIntegerLittleEndian_ShouldReturnFrames()
    {
        var reader = new PcmDataReader(AudioFormat.Signed16BitIntegerLittleEndian, new VolumeFilter(100))
        {
            Data = new byte[] { 1, 50, 100, 255 }
        };

        var buffer = new float[10];
        var count = reader.ReadFrames(buffer, buffer.Length);

        count.Should().Be(2);
        buffer.Should().BeEquivalentTo([0.39065552f, -0.004760742f, 0, 0, 0, 0, 0, 0, 0, 0]);
    }

    [Fact]
    public void Float32BitLittleEndian_ShouldReturnFrames()
    {
        var reader = new PcmDataReader(AudioFormat.Float32BitLittleEndian, new VolumeFilter(100))
        {
            Data = new byte[] { 0, 0, 0, 63 }
        };

        var buffer = new float[10];
        var count = reader.ReadFrames(buffer, buffer.Length);

        count.Should().Be(1);
        buffer.Should().BeEquivalentTo([0.5f, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
    }
}