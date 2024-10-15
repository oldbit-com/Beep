using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using OldBit.Beep.Pcm;
using OldBit.Beep.Platforms.Linux.AlsaInterop;

namespace OldBit.Beep.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class AlsaPlayer : IAudioPlayer
{
    private IntPtr _pcm = IntPtr.Zero;

    internal AlsaPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        try
        {
            InitializePlayer();
        }
        catch (DllNotFoundException ex)
        {
            throw new AudioPlayerException("Make sure ALSA development library has been installed.", ex);
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

    }

    public void Stop()
    {
      //  InitializePlayer();

    }

    public void Dispose()
    {
        if (_pcm != IntPtr.Zero)
        {
            // Alsa.snd_pcm_close(_pcm);
           // _pcm = IntPtr.Zero;
        }
    }

    private void InitializePlayer()
    {
        var result = Alsa.snd_pcm_open(ref _pcm, "default", PcmStream.Playback, 0);
        ThrowIfError(result, "Unable to open PCM connection");

        var parameters = IntPtr.Zero;
        result = Alsa.snd_pcm_hw_params_malloc(ref parameters);
        ThrowIfError(result, "Unable to allocate parameters buffer");

        result = Alsa.snd_pcm_hw_params_set_access(_pcm, parameters, PcmAccess.ReadWriteInterleaved);
        ThrowIfError(result, "Unable to set access");

        result = Alsa.snd_pcm_hw_params_set_format(_pcm, parameters, PcmFormat.PcmFormatFloatLittleEndian);
        ThrowIfError(result, "Unable to set format");
    }

    private static void ThrowIfError(int result, string message)
    {
        if (result >= 0)
        {
            return;
        }

        var alsaMessage = Marshal.PtrToStringAnsi(Alsa.snd_strerror(result));
        throw new AudioPlayerException($"{message}: {alsaMessage}", result);
    }
}
