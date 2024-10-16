namespace OldBit.Beep.Platforms.Linux.AlsaInterop;

internal enum PcmStream
{
    Playback = 0,
    Capture = 1,
}

internal enum PcmFormat
{
    /// <summary>
    /// SND_PCM_FORMAT_FLOAT_LE
    /// </summary>
    FloatLittleEndian = 14,
}

internal enum PcmAccess
{
    /// <summary>
    /// SND_PCM_ACCESS_RW_INTERLEAVED
    /// </summary>
    ReadWriteInterleaved = 3,
}