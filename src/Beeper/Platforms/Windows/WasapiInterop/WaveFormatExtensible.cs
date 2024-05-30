using System.Runtime.InteropServices;
using OldBit.Beeper.Platforms.Windows.WasapiInterop.Enums;

namespace OldBit.Beeper.Platforms.Windows.WasapiInterop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
internal struct WaveFormatExtensible
{
    internal WaveFormat WaveFormat;

    internal ushort ValidBitsPerSample; // wSamplesPerBlock or wReserved

    internal ChannelMask ChannelMask;

    internal Guid SubFormat;
}
