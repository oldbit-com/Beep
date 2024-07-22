using System.Runtime.InteropServices;
using OldBit.Beep.Platforms.Windows.WasapiInterop.Enums;

namespace OldBit.Beep.Platforms.Windows.WasapiInterop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
internal struct WaveFormat
{
    internal WaveFormatTag FormatTag;

    internal short Channels;

    internal int SamplesPerSecond;

    internal int AverageBytesPerSecond;

    internal short BlockAlign;

    internal short BitsPerSample;

    internal short ExtraSize;
}
