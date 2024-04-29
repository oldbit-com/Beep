using System.Runtime.InteropServices;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct WaveFormat
{
    internal WaveFormatTag Tag;

    internal ushort Channels;

    internal uint SampleRate;

    internal uint AverageBytesPerSecond;

    internal ushort BlockAlign;

    internal ushort BitsPerSample;

    internal ushort Size;
}
