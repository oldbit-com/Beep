using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using OldBit.Beeper.Windows.CoreAudioInterop.Enums;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

[Guid(IID)]
[GeneratedComInterface(StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(BStrStringMarshaller))]
internal partial interface IMMDevice
{
    internal const string IID = "D666063F-1587-4E43-81F1-B948E807363F";

    IAudioClient Activate(ref Guid iid, ClsCtx clsCtx, IntPtr activationParams);

    IntPtr OpenPropertyStore(int stgmAccess);

    string GetId();

    int GetState();
}