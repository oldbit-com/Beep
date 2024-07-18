namespace Demo.Generator;

public interface IWaveGenerator
{
    IEnumerable<byte> Generate(float frequency, TimeSpan duration);
}