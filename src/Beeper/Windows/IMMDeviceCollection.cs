using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace OldBit.Beeper.Windows
{
    [GeneratedComInterface]
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    internal partial interface IMMDeviceCollection
    {
        int GetCount(out int numDevices);
    }
}
