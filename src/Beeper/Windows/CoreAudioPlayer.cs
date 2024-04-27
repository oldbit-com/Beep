using OldBit.Beeper.Windows.CoreAudioInterop;

namespace OldBit.Beeper.Windows;

public partial class CoreAudioPlayer : IAudioPlayer
{
   
    public CoreAudioPlayer()
    {
        var deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        var device = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);
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