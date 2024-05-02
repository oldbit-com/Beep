namespace OldBit.Beeper.Windows.WasapiInterop.Enums;

internal enum ChannelMask : uint
{
    SpeakerFrontLeft = 0x1,
    SpeakerFrontRight = 0x2,
    SpeakerFrontCenter = 0x4,

    Mono = SpeakerFrontCenter,
    Stereo = SpeakerFrontLeft | SpeakerFrontRight
}
