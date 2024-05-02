using System.Runtime.Versioning;
using OldBit.Beeper.IO;
using OldBit.Beeper.Windows.WasapiInterop;
using OldBit.Beeper.Windows.WasapiInterop.Enums;

namespace OldBit.Beeper.Windows;

[SupportedOSPlatform("windows")]
internal class CoreAudioPlayer : IAudioPlayer
{
    private readonly AudioClient _audioClient;
    private readonly IAudioRenderClient _renderClient;
    private readonly EventWaitHandle _frameEventWaitHandle = new(false, EventResetMode.AutoReset);

    internal CoreAudioPlayer(int sampleRate, int channelCount)
    {
        var device = GetDevice();
        _audioClient = GetAudioClient(device);

        var format = GetFormat(sampleRate, channelCount);
        Initialize(format);

        var bufferSize = _audioClient.GetBufferSize() - _audioClient.GetCurrentPadding();
        bufferSize = (int)Math.Floor(bufferSize / 16f) * 16;
        BufferSizeInBytes = bufferSize;

        _renderClient = _audioClient.GetService();
    }

    private static IMMDevice GetDevice()
    {
        var deviceEnumerator = ClassActivator.Activate<IMMDeviceEnumerator>(IMMDeviceEnumerator.CLSID, IMMDeviceEnumerator.IID);

        return deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);
    }

    private static AudioClient GetAudioClient(IMMDevice device)
    {
        var audioClientId = new Guid(IAudioClient.IID);
        var audioClient = device.Activate(ref audioClientId, ClsCtx.All, IntPtr.Zero);

        return new AudioClient(audioClient);
    }

    private static WaveFormatExtensible GetFormat(int sampleRate, int channelCount)
    {
        var blockAlign = 32 * channelCount / 8;

        return new WaveFormatExtensible
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
            SubFormat = SubFormat.IeeeFloat
        };
    }

    private void Initialize(WaveFormatExtensible format)
    {
        _audioClient.Initialize(
            AudioClientShareMode.Shared,
            AudioClientStreamFlags.EventCallback | AudioClientStreamFlags.NoPersist | AudioClientStreamFlags.AutoConvertPCM,
            TimeSpan.FromMilliseconds(100), format);

        _audioClient.SetEventHandle(_frameEventWaitHandle.SafeWaitHandle.DangerousGetHandle());
    }

    public void Start()
    {
        _audioClient.Start();
    }

    public void Stop()
    {
        _audioClient.Stop();
    }

    public Task Play(PcmDataReader reader, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Enqueue(float[] pcmData, CancellationToken cancellationToken = default)
    {
        var waitHandles = new WaitHandle[] { _frameEventWaitHandle };

        WaitHandle.WaitAny(waitHandles);

        var buffer = _renderClient.GetBuffer(BufferSizeInBytes);

        return Task.CompletedTask;
    }

    public int BufferSizeInBytes { get; }

    public void Dispose()
    {
        _frameEventWaitHandle.Dispose();
    }
}