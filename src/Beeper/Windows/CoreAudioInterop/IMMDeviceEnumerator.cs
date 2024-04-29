using System.Runtime.InteropServices;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

[ComImport]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceEnumerator
{
    IMMDeviceCollection EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask);

    IMMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role);

    IMMDevice GetDevice(string id);

    int RegisterEndpointNotificationCallback(IMMNotificationClient client);

    int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
}

[ComImport]
[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
internal class MMDeviceEnumerator
{
}