using OldBit.Beeper.Windows.CoreAudioInterop;
using OldBit.Beeper.Windows.CoreAudioInterop.Enums;
using System.Runtime.Versioning;

namespace OldBit.Beeper.Windows;

[SupportedOSPlatform("windows")]
internal class CoreAudioPlayer : IAudioPlayer
{
    private readonly IAudioClient _audioClient;

    internal CoreAudioPlayer(int sampleRate, int channelCount)
    {
        var deviceEnumerator = ClassActivator.Activate<IMMDeviceEnumerator>(IMMDeviceEnumerator.CLSID, IMMDeviceEnumerator.IID);

        var device = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);

        var audioClientId = new Guid(IAudioClient.IID);
        _audioClient = device.Activate(ref audioClientId, ClsCtx.All, IntPtr.Zero);

        var blockAlign = 32 * channelCount / 8;
        var waveFormat = new WaveFormatExtensible
        {
            WaveFormat = new WaveFormat
            {
                FormatTag = WaveFormatTag.Extensible,
                Channels = (ushort)channelCount,
                SamplesPerSecond = (uint)sampleRate,
                AverageBytesPerSecond = (uint)(sampleRate * blockAlign),
                BlockAlign = (ushort)blockAlign,
                BitsPerSample = 32,
                ExtraSize = 22
            },
            ValidBitsPerSample = 32,
            ChannelMask = ChannelMask.Stereo,
            SubFormat = new Guid("00000003-0000-0010-8000-00aa00389b71") // KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
        };
        
        unsafe
        {
            WaveFormatExtensible supportedWaveFormat;
            var result = _audioClient.IsFormatSupported(AudioClientShareMode.Shared, waveFormat, new IntPtr(&supportedWaveFormat));
        }
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