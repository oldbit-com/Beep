using System.Runtime.InteropServices;
using OldBit.Beep.Platforms.Windows.WasapiInterop;
using OldBit.Beep.Platforms.Windows.WasapiInterop.Enums;

namespace OldBit.Beep.Platforms.Windows;

internal class AudioClient
{
    private readonly IAudioClient _audioClient;

    internal AudioClient(IAudioClient audioClient)
    {
        _audioClient = audioClient;
    }

    internal int IsFormatSupported(AudioClientShareMode shareMode, WaveFormatExtensible format, out WaveFormatExtensible? closestMatch)
    {
        closestMatch = null;

        var closestMatchPtrToPtr = Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>());
        var result = _audioClient.IsFormatSupported(AudioClientShareMode.Shared, format, closestMatchPtrToPtr);

        var closestMatchPtr = Marshal.PtrToStructure<IntPtr>(closestMatchPtrToPtr);
        if (closestMatchPtr != IntPtr.Zero)
        {
            closestMatch = Marshal.PtrToStructure<WaveFormatExtensible>(closestMatchPtr);
            Marshal.FreeCoTaskMem(closestMatchPtr);
        }

        Marshal.FreeHGlobal(closestMatchPtrToPtr);

        return result;
    }

    internal void Initialize(AudioClientShareMode shareMode, AudioClientStreamFlags streamFlags, TimeSpan bufferDuration, WaveFormatExtensible format)
    {
        var bufferSize = (long)(TimeSpan.FromMilliseconds(100).TotalNanoseconds / 100);
        var audioSessionId = Guid.Empty;

        _audioClient.Initialize(
            AudioClientShareMode.Shared,
            AudioClientStreamFlags.EventCallback | AudioClientStreamFlags.NoPersist | AudioClientStreamFlags.AutoConvertPCM,
            bufferSize, 0, format, ref audioSessionId);
    }

    internal IAudioRenderClient GetService()
    {
        var audioRenderClientId = new Guid(IAudioRenderClient.IID);

        return _audioClient.GetService(ref audioRenderClientId);
    }

    internal int GetBufferSize() => (int)_audioClient.GetBufferSize();

    internal int GetCurrentPadding() => (int)_audioClient.GetCurrentPadding();

    internal void Start() => _audioClient.Start();

    internal void Stop() => _audioClient.Stop();

    internal void SetEventHandle(IntPtr eventHandle) => _audioClient.SetEventHandle(eventHandle);
}
