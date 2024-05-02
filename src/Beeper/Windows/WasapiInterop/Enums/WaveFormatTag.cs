namespace OldBit.Beeper.Windows.WasapiInterop.Enums;

internal enum WaveFormatTag : ushort
{
    Unknown = 0x0000,

    Pcm = 0x0001,

    Adpcm = 0x0002,

    IeeeFloat = 0x0003,

    Extensible = 0xFFFE,
}
