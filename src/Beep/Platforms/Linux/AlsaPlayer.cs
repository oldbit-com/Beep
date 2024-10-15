using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using OldBit.Beep.Helpers;
using OldBit.Beep.Pcm;
using OldBit.Beep.Platforms.Linux.AlsaInterop;

namespace OldBit.Beep.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class AlsaPlayer : IAudioPlayer
{
    private IntPtr _pcm = IntPtr.Zero;

    internal AlsaPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        const int period = 2;

        var periodSize = playerOptions.BufferSizeInBytes / (channelCount * FloatType.SizeInBytes * period);
        var bufferSize = periodSize * period;

        try
        {
            InitializePlayer(sampleRate, channelCount, (ulong)periodSize, (ulong)bufferSize);
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

    private void InitializePlayer(int sampleRate, int channelCount, ulong periodSize, ulong bufferSize)
    {
        var result = Alsa.snd_pcm_open(ref _pcm, "default", PcmStream.Playback, 0);
        ThrowIfError(result, "Unable to open PCM connection");

        var parameters = IntPtr.Zero;
        result = Alsa.snd_pcm_hw_params_malloc(ref parameters);
        ThrowIfError(result, "Unable to allocate parameters buffer");

        result = Alsa.snd_pcm_hw_params_any(_pcm, parameters);
        ThrowIfError(result, "Unable to allocate parameters buffer");

        result = Alsa.snd_pcm_hw_params_set_access(_pcm, parameters, PcmAccess.ReadWriteInterleaved);
        ThrowIfError(result, "Unable to set access");

        result = Alsa.snd_pcm_hw_params_set_format(_pcm, parameters, PcmFormat.PcmFormatFloatLittleEndian);
        ThrowIfError(result, "Unable to set format");

        result = Alsa.snd_pcm_hw_params_set_channels(_pcm, parameters, (uint)channelCount);
        ThrowIfError(result, "Unable to set channels");

        unsafe
        {
            var rate = (uint)sampleRate;

            result = Alsa.snd_pcm_hw_params_set_rate_near(_pcm, parameters, &rate, null);
            ThrowIfError(result, "Unable to set sample rate");

            result = Alsa.snd_pcm_hw_params_set_buffer_size_near(_pcm, parameters, &bufferSize);
            ThrowIfError(result, "Unable to set buffer size");

            result = Alsa.snd_pcm_hw_params_set_period_size_near(_pcm, parameters, &periodSize, null);
            ThrowIfError(result, "Unable to set period size");
        }

        result = Alsa.snd_pcm_hw_params(_pcm, parameters);
        ThrowIfError(result, "Unable to send Alsa params");
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
