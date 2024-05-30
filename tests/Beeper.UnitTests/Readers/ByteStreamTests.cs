using FluentAssertions;
using OldBit.Beeper.Readers;

namespace OldBit.Beeper.UnitTests.Readers;

public class ByteStreamTests
{
    [Fact]
    public void Read_ShouldReturnCorrectNumberOfBytes()
    {
        var data = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
        using var stream = new ByteStream(data);
        var buffer = new byte[200];

        var bytesRead = stream.Read(buffer, 0, buffer.Length);

        bytesRead.Should().Be(data.Length);
        buffer.Take(100).Should().BeEquivalentTo(data.Take(100));
        buffer.Skip(100).Should().AllSatisfy(x => x.Should().Be(0));
    }
}