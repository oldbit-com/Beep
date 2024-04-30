using Microsoft.VisualBasic;
using OldBit.Beeper.Windows.CoreAudioInterop;
using OldBit.Beeper.Windows.CoreAudioInterop.Enums;
using System.Runtime.InteropServices;

namespace OldBit.Beeper.Windows;

internal class AudioClient
{
    private readonly IAudioClient _audioClient;

    internal AudioClient(IAudioClient audioClient)
    {
        _audioClient = audioClient;
    }

    internal int IsFormatSupported(AudioClientShareMode shareMode, WaveFormatExtensible waveFormat, out WaveFormatExtensible? closestMatch)
    {
        closestMatch = null;

        var closestMatchPtrToPtr = Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>());
        var result = _audioClient.IsFormatSupported(AudioClientShareMode.Shared, waveFormat, closestMatchPtrToPtr);

        var closestMatchPtr = Marshal.PtrToStructure<IntPtr>(closestMatchPtrToPtr);
        if (closestMatchPtr != IntPtr.Zero)
        {
            closestMatch = Marshal.PtrToStructure<WaveFormatExtensible>(closestMatchPtr);
            Marshal.FreeCoTaskMem(closestMatchPtr);
        }
        Marshal.FreeHGlobal(closestMatchPtrToPtr);

        return result;
    }

    internal void Start() => _audioClient.Start();

    internal void Stop() => _audioClient.Stop();
}
