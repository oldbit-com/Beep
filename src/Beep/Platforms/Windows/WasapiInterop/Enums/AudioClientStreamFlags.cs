namespace OldBit.Beep.Platforms.Windows.WasapiInterop.Enums;

[Flags]
internal enum AudioClientStreamFlags : uint
{
    None = 0,

    CrossProcess = 0x00010000,

    Loopback = 0x00020000,

    EventCallback = 0x00040000,

    NoPersist = 0x00080000,

    RateAdjust = 0x00100000,

    AutoConvertPCM = 0x80000000,

    SrcDefaultQuality = 0x08000000,
}
