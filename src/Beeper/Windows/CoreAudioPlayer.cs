using OldBit.Beeper.Windows.CoreAudioInterop;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OldBit.Beeper.Windows;

[SupportedOSPlatform("windows")]
public class CoreAudioPlayer : IAudioPlayer
{
    public CoreAudioPlayer()
    {
        var deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        var device = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);

        _ = Marshal.ReleaseComObject(deviceEnumerator);
        _ = Marshal.ReleaseComObject(device);
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public Task Enqueue(float[] data, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }
}