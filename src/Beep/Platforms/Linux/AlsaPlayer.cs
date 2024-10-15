using System.Runtime.Versioning;
using OldBit.Beep.Pcm;
using OldBit.Beep.Platforms.Linux.AlsaInterop;

namespace OldBit.Beep.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class AlsaPlayer : IAudioPlayer
{
    internal AlsaPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        var @params = new IntPtr();

        try
        {
            var result = Alsa.snd_pcm_hw_params_malloc(ref @params);
        }
        catch (DllNotFoundException ex)
        {
            throw new AudioPlayerException("Mke sure 'libasound2-dev' library has been installed.", ex);
        }
    }

    public ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public bool TryEnqueue(PcmDataReader reader)
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
