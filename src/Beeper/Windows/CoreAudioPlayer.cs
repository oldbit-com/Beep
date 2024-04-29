using OldBit.Beeper.Windows.CoreAudioInterop;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OldBit.Beeper.Windows;

[SupportedOSPlatform("windows")]
internal class CoreAudioPlayer : IAudioPlayer
{
    private readonly IAudioClient _audioClient;

    internal CoreAudioPlayer()
    {
        var deviceEnumerator = ClassActivator.Activate<IMMDeviceEnumerator>(IMMDeviceEnumerator.CLSID, IMMDeviceEnumerator.IID);

        var device = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);

        var audioClientId = new Guid(IAudioClient.IID);
        _audioClient = device.Activate(ref audioClientId, ClsCtx.All, IntPtr.Zero);
    }

    public void Start()
    {
        _audioClient.Start();
    }

    public void Stop()
    {
        _audioClient.Stop();
    }

    public Task Enqueue(float[] data, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}