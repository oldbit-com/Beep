using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

[Guid(IID)]
[GeneratedComInterface(StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(BStrStringMarshaller))]
internal partial interface IMMDeviceEnumerator
{
    internal const string IID = "A95664D2-9614-4F35-A746-DE8DB63617E6";
    internal const string CLSID = "BCDE0395-E52F-467C-8E3D-C4579291692E";

    IMMDeviceCollection EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask);

    IMMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role);

    IMMDevice GetDevice(string id);

    void RegisterEndpointNotificationCallback(IMMNotificationClient client);

    void UnregisterEndpointNotificationCallback([MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client);
}
