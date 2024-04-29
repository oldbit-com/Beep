using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(BStrStringMarshaller))]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
internal partial interface IMMDevice
{
    void Activate(ref Guid iid, ClsCtx clsCtx, IntPtr activationParams, out IntPtr ppInterface);

    IntPtr OpenPropertyStore(int stgmAccess);

    string GetId();

    int GetState();
}