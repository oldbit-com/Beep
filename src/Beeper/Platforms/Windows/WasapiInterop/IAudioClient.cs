using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using OldBit.Beeper.Platforms.Windows.WasapiInterop.Enums;

namespace OldBit.Beeper.Platforms.Windows.WasapiInterop;

[Guid(IID)]
[GeneratedComInterface]
internal partial interface IAudioClient
{
    internal const string IID = "1CB9AD4C-DBFA-4c32-B178-C2F568A703B2";

    void Initialize(AudioClientShareMode shareMode, AudioClientStreamFlags streamFlags, long hnsBufferDuration, long hnsPeriodicity, WaveFormatExtensible pFormat, ref Guid audioSessionGuid);

    uint GetBufferSize();

    long GetStreamLatency();

    uint GetCurrentPadding();

    [PreserveSig]
    int IsFormatSupported(AudioClientShareMode shareMode, WaveFormatExtensible pFormat, IntPtr ppClosestMatch);

    IntPtr GetMixFormat();

    void GetDevicePeriod(out long defaultDevicePeriod, out long minimumDevicePeriod);

    void Start();

    void Stop();

    void Reset();

    void SetEventHandle(IntPtr eventHandle);

    IAudioRenderClient GetService(ref Guid interfaceId);
}
