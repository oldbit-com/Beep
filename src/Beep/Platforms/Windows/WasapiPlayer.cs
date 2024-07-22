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
    private const int RefTimesPerMillisecond = 10_000;

    private readonly IAudioClient _audioClient;
    private readonly IAudioRenderClient _renderClient;
    private readonly WaitHandle[] _waitHandles = [new EventWaitHandle(false, EventResetMode.AutoReset)];
    private readonly int _bufferFrameCount;
    private readonly int _frameSize;
    private readonly TimeSpan _halfBufferDuration;

    internal CoreAudioPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        _audioClient = Activate();

        Initialize(sampleRate, channelCount, playerOptions.BufferDuration);

        _bufferFrameCount = _audioClient.GetBufferSize();
        _frameSize = channelCount * FloatType.SizeInBytes;

        var bufferDuration = (double)RefTimesPerSecond * _bufferFrameCount / sampleRate;
        _halfBufferDuration = TimeSpan.FromMilliseconds(bufferDuration / RefTimesPerMillisecond / 2);

        var audioRenderClientId = new Guid(IAudioRenderClient.IID);
        _renderClient = _audioClient.GetService(ref audioRenderClientId);
    }

    private static IAudioClient Activate()
    {
        var deviceEnumerator = ClassActivator.Activate<IMMDeviceEnumerator>(IMMDeviceEnumerator.CLSID, IMMDeviceEnumerator.IID);
        var device = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);

        var audioClientId = new Guid(IAudioClient.IID);
        var audioClient = device.Activate(ref audioClientId, ClsCtx.All, IntPtr.Zero);

        return audioClient;
    }

    private void Initialize(int sampleRate, int channelCount, TimeSpan bufferDuration)
    {
        var waveFormat = GetWaveFormat(sampleRate, channelCount);

        var streamFlags =
            AudioClientStreamFlags.EventCallback |
            AudioClientStreamFlags.NoPersist |
            AudioClientStreamFlags.AutoConvertPCM;

        var audioSessionId = Guid.Empty;

        _audioClient.Initialize(
            AudioClientShareMode.Shared,
            streamFlags,
            (long)(bufferDuration.TotalNanoseconds / 100), // 100ns units
            0,
            waveFormat,
            ref audioSessionId);


        _audioClient.SetEventHandle(_waitHandles[0].SafeWaitHandle.DangerousGetHandle());
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

    public void Start()
    {
        _audioClient.Start();
    }

    public void Stop()
    {
        _audioClient.Stop();
    }

    public async Task PlayAsync(PcmDataReader reader, CancellationToken cancellationToken)
    {
        var sourceBuffer = new float[_bufferFrameCount * _frameSize];

        await Task.Run(async () =>
        {
            var firstFrame = true;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!firstFrame)
                {
                    await Task.Delay(_halfBufferDuration, cancellationToken);
                }

                WaitHandle.WaitAny(_waitHandles);

                var paddingFrameCount = _audioClient.GetCurrentPadding();
                var framesAvailable = _bufferFrameCount - paddingFrameCount;

                var samplesCount = reader.ReadFrames(sourceBuffer, framesAvailable);
                if (samplesCount == 0)
                {
                    break;
                }

                var audioBuffer = _renderClient.GetBuffer(framesAvailable);

                Marshal.Copy(sourceBuffer, 0, audioBuffer, samplesCount);

                _renderClient.ReleaseBuffer(framesAvailable, AudioClientBufferFlags.None);

                firstFrame = false;
            }
        }, cancellationToken);

        await Task.Delay(_halfBufferDuration, cancellationToken);
    }

    public void Dispose()
    {
        _waitHandles[0].Dispose();
    }
}