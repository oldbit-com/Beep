using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

[Guid(IID)]
[GeneratedComInterface]
internal partial interface IMMDeviceCollection
{
    internal const string IID = "0BD7A1BE-7A1A-44DB-8397-CC5392387B5E";

    int GetCount(out int numDevices);
}