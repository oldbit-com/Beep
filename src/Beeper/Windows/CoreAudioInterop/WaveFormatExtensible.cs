using OldBit.Beeper.Windows.CoreAudioInterop.Enums;
using System.Runtime.InteropServices;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
internal struct WaveFormatExtensible
{
    internal WaveFormat WaveFormat;

    internal ushort ValidBitsPerSample; // wSamplesPerBlock or wReserved

    internal ChannelMask ChannelMask;

    internal Guid SubFormat;
}
