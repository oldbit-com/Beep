using System.Runtime.InteropServices;

namespace OldBit.Beep.Platforms.Linux.AlsaInterop;

internal static partial class Alsa
{
    private const string AlsaLibrary = "libasound";

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_open(ref IntPtr pcm, [MarshalAs(UnmanagedType.LPStr)]  string name, PcmStream stream, int mode);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_writei(IntPtr pcm, IntPtr buffer, ulong size);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_writei(IntPtr pcm, float[] buffer, ulong size);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_recover(IntPtr pcm, int err, int silent);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_prepare(IntPtr pcm);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_close(IntPtr pcm);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params(IntPtr pcm, IntPtr @params);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_malloc(ref IntPtr @params);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_free(IntPtr @params);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_any(IntPtr pcm, IntPtr @params);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_set_access(IntPtr pcm, IntPtr @params, PcmAccess access);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_set_format(IntPtr pcm, IntPtr @params, PcmFormat val);

    [LibraryImport(AlsaLibrary)]
    internal static partial IntPtr snd_strerror(int errnum);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_set_channels(IntPtr pcm, IntPtr @params, uint val);

    [LibraryImport(AlsaLibrary)]
    //internal static unsafe partial int snd_pcm_hw_params_set_rate_near(IntPtr pcm, IntPtr @params, uint* val, int* dir);
    internal static partial int snd_pcm_hw_params_set_rate_near(IntPtr pcm, IntPtr @params, ref uint val, ref int dir);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_set_buffer_size_near(IntPtr pcm, IntPtr @params, ref ulong val);

    [LibraryImport(AlsaLibrary)]
    internal static partial int snd_pcm_hw_params_set_period_size_near(IntPtr pcm, IntPtr @params, ref ulong val, ref int dir);
}