using OldBit.Beep.Filters;
using OldBit.Beep.Pcm;
using Shouldly;

namespace OldBit.Beep.Tests.Pcm;

public class PcmDataReaderTests
{
    [Fact]
    public void Unsigned8Bit_ShouldReturnFloatSamples()
    {
        var reader = new PcmDataReader(AudioFormat.Unsigned8Bit, new VolumeFilter(100));
        reader.SetData([1, 50, 100, 255]);

        var buffer = new float[10];
        var count = reader.ReadSamples(buffer, buffer.Length);

        count.ShouldBe(4);
        buffer.ShouldBeEquivalentTo(new[] { -0.9921875f, -0.609375f, -0.21875f, 0.9921875f, 0, 0, 0, 0, 0, 0 });
    }

    [Fact]
    public void Signed16BitIntegerLittleEndian_ShouldReturnFloatSamples()
    {
        var reader = new PcmDataReader(AudioFormat.Signed16BitIntegerLittleEndian, new VolumeFilter(100));
        reader.SetData([1, 50, 100, 255]);

        var buffer = new float[10];
        var count = reader.ReadSamples(buffer, buffer.Length);

        count.ShouldBe(2);
        buffer.ShouldBeEquivalentTo(new[] { 0.39065552f, -0.004760742f, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    [Fact]
    public void Float32BitLittleEndian_ShouldReturnFloatSamples()
    {
        var reader = new PcmDataReader(AudioFormat.Float32BitLittleEndian, new VolumeFilter(100));
        reader.SetData([0, 0, 0, 63]);

        var buffer = new float[10];
        var count = reader.ReadSamples(buffer, buffer.Length);

        count.ShouldBe(1);
        buffer.ShouldBeEquivalentTo(new[] { 0.5f, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
    }
}