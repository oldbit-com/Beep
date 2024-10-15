using System.Runtime.InteropServices;

namespace OldBit.Beep.Platforms.Linux.AlsaInterop;

internal static partial class Alsa
{
    private const string AlsaLibrary = "libasound";

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_open(ref IntPtr pcm, [MarshalAs(UnmanagedType.LPStr)]  string name, PcmStream stream, int mode);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_malloc(ref IntPtr @params);
}