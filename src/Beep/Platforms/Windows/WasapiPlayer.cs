using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
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
    private readonly AsyncPauseResume _asyncPauseResume = new();
    private readonly Channel<PcmDataReader> _queue = Channel.CreateBounded<PcmDataReader>(100);
    private readonly Thread _queueWorker;
    
    private bool _isQueueRunning;
    private bool _isBufferEmpty;

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

        _queueWorker = CreateWorker();
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

        const AudioClientStreamFlags streamFlags = AudioClientStreamFlags.NoPersist |
                                                   AudioClientStreamFlags.AutoConvertPCM;
        var audioSessionId = Guid.Empty;

        _audioClient.Initialize(
            AudioClientShareMode.Shared,
            streamFlags,
            bufferSize100Ns,
            0,
            waveFormat,
            ref audioSessionId);
    }

    private long CalculateBufferSize100Ns(int sampleRate, int bufferSizeInBytes)
    {
        var bufferSizeInFrames = bufferSizeInBytes / _frameSize;

        return (long)RefTimesPerSecond * bufferSizeInFrames / sampleRate;
    }

    private static WaveFormatExtensible GetWaveFormat(int sampleRate, int channelCount)
    {
        var blockAlign = sizeof(float) * channelCount;

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
            ChannelMask = channelCount == 1 ? ChannelMask.Mono : ChannelMask.Stereo,
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
                while (!_isBufferEmpty)
                {
                    await _asyncPauseResume.WaitIfPausedAsync(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    await EnqueueAsync(reader, cancellationToken);
                }
            }, cancellationToken);
        }
        finally
        {
            Stop();
        }
    }

    private Thread CreateWorker() => new(() =>
    {
        while (_isQueueRunning)
        {
            if (!_queue.Reader.TryRead(out var samples))
            {
               // Thread.Sleep(10);
                continue;
            }

            while (_isQueueRunning)
            {
                var paddingFrameCount = _audioClient.GetCurrentPadding();
                var framesAvailable = _bufferFrameCount - paddingFrameCount;

                if (framesAvailable == 0)
                {
                    continue;
                }

                var audioDataLength = samples.ReadFrames(_audioData, framesAvailable * _channelCount);

                _isBufferEmpty = audioDataLength == 0;
                if (_isBufferEmpty)
                {
                    break;
                }

                // Adjust number of frames available based on the number of samples read
                framesAvailable = audioDataLength / _channelCount;

                var audioBuffer = _renderClient.GetBuffer(framesAvailable);
                Marshal.Copy(_audioData, 0, audioBuffer, audioDataLength);
                _renderClient.ReleaseBuffer(framesAvailable, AudioClientBufferFlags.None);
            }

            samples.Close();
        }
    })
    {
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal
    };

    public async Task EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken) =>
        await _queue.Writer.WriteAsync(reader);

    public void Start()
    {
        _audioClient.Start();
        _isQueueRunning = true;
        _queueWorker.Start();
    }

    public void Stop()
    {
        _isQueueRunning = false;
        _audioClient.Stop();
        _audioClient.Reset();
    }

    public void Pause() => _asyncPauseResume.Pause();

    public void Resume() => _asyncPauseResume.Resume();

    public void Dispose()
    {
    }
}