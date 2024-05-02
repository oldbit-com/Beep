using OldBit.Beeper.Extensions;
using OldBit.Beeper.Helpers;

namespace OldBit.Beeper.UnitTests;

public class AudioFormatHelperTests
{
    [Theory]
    [InlineData(AudioFormat.Unsigned8Bit, 1)]
    [InlineData(AudioFormat.Signed16BitIntegerLittleEndian, 2)]
    [InlineData(AudioFormat.Float32BitLittleEndian, 4)]
    public void GetByteSize_ShouldReturnCorrectNumberOfBytes(AudioFormat audioFormat, int expectedBytes)
    {
        var numberOfBytes = audioFormat.GetByteSize();

        Assert.Equal(expectedBytes, numberOfBytes);
    }
}