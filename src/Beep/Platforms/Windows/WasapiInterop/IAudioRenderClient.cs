using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using OldBit.Beep.Platforms.Windows.WasapiInterop.Enums;

namespace OldBit.Beep.Platforms.Windows.WasapiInterop;

[Guid(IID)]
[GeneratedComInterface]
internal partial interface IAudioRenderClient
{
    internal const string IID = "F294ACFC-3146-4483-A7BF-ADDCA7C260E2";

    IntPtr GetBuffer(int numFramesRequested);

    void ReleaseBuffer(int numFramesWritten, AudioClientBufferFlags bufferFlags);
}
