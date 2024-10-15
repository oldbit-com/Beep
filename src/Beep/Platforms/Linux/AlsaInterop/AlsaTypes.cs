namespace OldBit.Beep.Platforms.Linux.AlsaInterop;

internal enum PcmStream
{
    Playback = 0,
    Capture = 1,
}

internal enum PcmFormat
{
    PcmFormatFloatLittleEndian = 14,
}

internal enum PcmAccess
{
    ReadWriteInterleaved = 3,
}