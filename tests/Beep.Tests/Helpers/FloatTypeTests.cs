using OldBit.Beep.Extensions;
using Shouldly;

namespace OldBit.Beep.Tests.Helpers;

public class FloatTypeTests
{
    [Theory]
    [InlineData(AudioFormat.Unsigned8Bit, 1)]
    [InlineData(AudioFormat.Signed16BitIntegerLittleEndian, 2)]
    [InlineData(AudioFormat.Float32BitLittleEndian, 4)]
    public void GetByteSize_ShouldReturnCorrectNumberOfBytes(AudioFormat audioFormat, int expectedBytes)
    {
        var numberOfBytes = audioFormat.GetByteSize();

        numberOfBytes.ShouldBe(expectedBytes);
    }
}