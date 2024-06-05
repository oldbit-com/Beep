using System.Runtime.InteropServices;
using OldBit.Beep.Platforms.Windows.WasapiInterop.Enums;

namespace OldBit.Beep.Platforms.Windows.WasapiInterop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
internal struct WaveFormat
{
    internal WaveFormatTag FormatTag;

    internal ushort Channels;

    internal uint SamplesPerSecond;

    internal uint AverageBytesPerSecond;

    internal ushort BlockAlign;

    internal ushort BitsPerSample;

    internal ushort ExtraSize;
}
