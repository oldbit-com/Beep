using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using OldBit.Beep.Helpers;
using OldBit.Beep.Platforms.Windows.WasapiInterop;
using OldBit.Beep.Platforms.Windows.WasapiInterop.Enums;
using OldBit.Beep.Readers;

namespace OldBit.Beep.Platforms.Windows;

[SupportedOSPlatform("windows")]
internal class CoreAudioPlayer : IAudioPlayer
{
    private const int RefTimesPerSecond = 10_000_000;

    private readonly IAudioClient _audioClient;
    private readonly IAudioRenderClient _renderClient;
    private readonly int _bufferFrameCount;
    private readonly int _channelCount;
    private readonly int _frameSize;
    private readonly float[] _audioData;
    private readonly TimeSpan _waitTimeOut = TimeSpan.FromSeconds(2);
    private readonly AutoResetEvent _bufferReadyEvent = new(false);
    private readonly AsyncPauseResume _asyncPauseResume = new();

    internal CoreAudioPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        _audioClient = Activate();
        _frameSize = channelCount * FloatType.SizeInBytes;
        _channelCount = channelCount;

        Initialize(sampleRate, channelCount, playerOptions.BufferSizeInBytes);

        _bufferFrameCount = _audioClient.GetBufferSize();

        var audioRenderClientId = new Guid(IAudioRenderClient.IID);
        _renderClient = _audioClient.GetService(ref audioRenderClientId);

        _audioData = new float[_bufferFrameCount * _frameSize];
    }

    private static IAudioClient Activate()
    {
        var deviceEnumerator = ClassActivator.Activate<IMMDeviceEnumerator>(IMMDeviceEnumerator.CLSID, IMMDeviceEnumerator.IID);
        var device = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);

        var audioClientId = new Guid(IAudioClient.IID);
        var audioClient = device.Activate(ref audioClientId, ClsCtx.All, IntPtr.Zero);

        return audioClient;
    }

    private void Initialize(int sampleRate, int channelCount, int bufferSizeInBytes)
    {
        var waveFormat = GetWaveFormat(sampleRate, channelCount);
        var bufferSize100Ns = CalculateBufferSize100Ns(sampleRate, bufferSizeInBytes);

        const AudioClientStreamFlags streamFlags = AudioClientStreamFlags.EventCallback |
                                                   AudioClientStreamFlags.NoPersist |
                                                   AudioClientStreamFlags.AutoConvertPCM;
        var audioSessionId = Guid.Empty;

        _audioClient.Initialize(
            AudioClientShareMode.Shared,
            streamFlags,
            bufferSize100Ns,
            0,
            waveFormat,
            ref audioSessionId);

        _audioClient.SetEventHandle(_bufferReadyEvent.SafeWaitHandle.DangerousGetHandle());
    }

    private int CalculateBufferSize100Ns(int sampleRate, int bufferSizeInBytes)
    {
        var bufferSizeInFrames = bufferSizeInBytes / _frameSize;

        return RefTimesPerSecond * bufferSizeInFrames / sampleRate;
    }

    private static WaveFormatExtensible GetWaveFormat(int sampleRate, int channelCount)
    {
        var blockAlign = 32 * channelCount / 8;

        return new WaveFormatExtensible
        {
            WaveFormat = new WaveFormat
            {
                FormatTag = WaveFormatTag.Extensible,
                Channels = (short)channelCount,
                SamplesPerSecond = sampleRate,
                AverageBytesPerSecond = sampleRate * blockAlign,
                BlockAlign = (short)blockAlign,
                BitsPerSample = 32,
                ExtraSize = 22
            },
            ValidBitsPerSample = 32,
            ChannelMask = ChannelMask.Stereo,
            SubFormat = SubFormat.IeeeFloat
        };
    }

    public async Task PlayAsync(PcmDataReader reader, CancellationToken cancellationToken)
    {
        Start();

        try
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await _asyncPauseResume.WaitIfPausedAsync(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    await Enqueue(reader, cancellationToken);
                }
            }, cancellationToken);
        }
        finally
        {
            Stop();
        }
    }

    public Task Enqueue(PcmDataReader reader, CancellationToken cancellationToken)
    {
        _bufferReadyEvent.WaitOne(_waitTimeOut, false);

        var paddingFrameCount = _audioClient.GetCurrentPadding();
        var framesAvailable = _bufferFrameCount - paddingFrameCount;

        var audioDataLength = reader.ReadFrames(_audioData, framesAvailable * _channelCount);
        if (audioDataLength == 0)
        {
            return Task.CompletedTask;
        }

        var audioBuffer = _renderClient.GetBuffer(framesAvailable);

        Marshal.Copy(_audioData, 0, audioBuffer, audioDataLength);

        _renderClient.ReleaseBuffer(framesAvailable, AudioClientBufferFlags.None);

        return Task.CompletedTask;
    }

    public void Start() => _audioClient.Start();

    public void Stop()
    {
        _audioClient.Stop();
        _audioClient.Reset();
    }

    public void Pause() => _asyncPauseResume.Pause();

    public void Resume() => _asyncPauseResume.Resume();

    public void Dispose()
    {
        _bufferReadyEvent.Dispose();
    }
}