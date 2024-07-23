using System.Diagnostics;
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
    private readonly int _frameSize;
    private readonly AutoResetEvent _bufferReadyEvent = new(false);

    internal CoreAudioPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        _audioClient = Activate();
        _frameSize = channelCount * FloatType.SizeInBytes;

        Initialize(sampleRate, channelCount, playerOptions.BufferSizeInBytes);

        _bufferFrameCount = _audioClient.GetBufferSize();

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

    private void Initialize(int sampleRate, int channelCount, int bufferSizeInBytes)
    {
        var waveFormat = GetWaveFormat(sampleRate, channelCount);
        var bufferSize100ns = CalculateBufferSize100ns(sampleRate, channelCount, bufferSizeInBytes);

        var streamFlags =
            AudioClientStreamFlags.EventCallback |
            AudioClientStreamFlags.NoPersist |
            AudioClientStreamFlags.AutoConvertPCM;

        var audioSessionId = Guid.Empty;

        _audioClient.Initialize(
            AudioClientShareMode.Shared,
            streamFlags,
            (long)bufferSize100ns,
            0,
            waveFormat,
            ref audioSessionId);

        _audioClient.SetEventHandle(_bufferReadyEvent.SafeWaitHandle.DangerousGetHandle());
    }

    private int CalculateBufferSize100ns(int sampleRate, int channelCount, int bufferSizeInBytes)
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

    public void Start()
    {
    }

    public void Stop()
    {
    }

    public async Task PlayAsync(PcmDataReader reader, CancellationToken cancellationToken)
    {
        _audioClient.Start();

        var sourceBuffer = new float[_bufferFrameCount * _frameSize];
        var waitTimeOut = TimeSpan.FromSeconds(2);

        try
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _bufferReadyEvent.WaitOne(waitTimeOut, false);

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
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _audioClient.Stop();
            _audioClient.Reset();
            
            throw;
        }
        finally
        {
            _audioClient.Stop();
            _audioClient.Reset();
        }
    }

    public void Dispose()
    {
        _bufferReadyEvent.Dispose();
    }
}