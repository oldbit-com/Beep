using OldBit.Beeper.Windows.CoreAudioInterop;
using OldBit.Beeper.Windows.CoreAudioInterop.Enums;
using System.Runtime.Versioning;

namespace OldBit.Beeper.Windows;

[SupportedOSPlatform("windows")]
internal class CoreAudioPlayer : IAudioPlayer
{
    private readonly AudioClient _audioClient;
    private readonly IAudioRenderClient _renderClient;
    private readonly int _bufferSize;
    private readonly EventWaitHandle _frameEventWaitHandle = new(false, EventResetMode.AutoReset);

    internal CoreAudioPlayer(int sampleRate, int channelCount)
    {
        var device = GetDevice();
        _audioClient = GetAudioClient(device);

        var format = GetFormat(sampleRate, channelCount);
        Initialize(format);

        var bufferSize = _audioClient.GetBufferSize() - _audioClient.GetCurrentPadding();
        bufferSize = (int)Math.Floor(bufferSize / 16f) * 16;
        _bufferSize = bufferSize;

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

    public Task Enqueue(float[] data, CancellationToken cancellationToken = default)
    {
        var waitHandles = new WaitHandle[] { _frameEventWaitHandle };

        var buffer = _renderClient.GetBuffer(BufferSize);   

        return Task.CompletedTask;
    }

    internal int BufferSize => _bufferSize;

    public void Dispose()
    {
        _frameEventWaitHandle.Dispose();
    }
}