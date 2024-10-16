using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
using OldBit.Beep.Helpers;
using OldBit.Beep.Pcm;
using OldBit.Beep.Platforms.Linux.AlsaInterop;
using static OldBit.Beep.Platforms.Linux.AlsaInterop.Alsa;

namespace OldBit.Beep.Platforms.Linux;

[SupportedOSPlatform("linux")]
internal sealed class AlsaPlayer : IAudioPlayer
{
    private const int Period = 2;

    private readonly int _frameSize;
    private readonly Channel<PcmDataReader> _samplesQueue;
    private readonly Thread _queueWorker;
    private readonly float[] _audioData;

    private IntPtr _pcm = IntPtr.Zero;
    private bool _isQueueRunning;
    private bool _queueWorkerStopped = true;

    internal AlsaPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        _frameSize = channelCount * FloatType.SizeInBytes;

        _samplesQueue = Channel.CreateBounded<PcmDataReader>(new BoundedChannelOptions(playerOptions.MaxQueueSize)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var periodSize = playerOptions.BufferSizeInBytes / (_frameSize * Period);
        var bufferSize = periodSize * Period;

        try
        {
            Initialize(sampleRate, channelCount, (ulong)periodSize, (ulong)bufferSize);
        }
        catch (DllNotFoundException ex)
        {
            throw new AudioPlayerException("Make sure ALSA development library has been installed.", ex);
        }

        _audioData = new float[playerOptions.BufferSizeInBytes];

        _queueWorker = CreateQueueWorkerThread();
    }

    public async ValueTask EnqueueAsync(PcmDataReader reader, CancellationToken cancellationToken) =>
        await _samplesQueue.Writer.WriteAsync(reader, cancellationToken);

    public bool TryEnqueue(PcmDataReader reader) => _samplesQueue.Writer.TryWrite(reader);

    public void Start()
    {
        _isQueueRunning = true;
        _queueWorker.Start();
    }

    public void Stop()
    {
        _isQueueRunning = false;

        while (!_queueWorkerStopped)
        {
            Thread.Sleep(5);
        }
        _queueWorker.Join();

    }

    public void Dispose()
    {
        if (_pcm != IntPtr.Zero)
        {
            // Alsa.snd_pcm_close(_pcm);
           // _pcm = IntPtr.Zero;
        }
    }

    private void Initialize(int sampleRate, int channelCount, ulong periodSize, ulong bufferSize)
    {
        var result = snd_pcm_open(ref _pcm, "default", PcmStream.Playback, 0);
        ThrowIfError(result, "Unable to open PCM connection");

        var parameters = IntPtr.Zero;
        result = snd_pcm_hw_params_malloc(ref parameters);
        ThrowIfError(result, "Unable to allocate parameters buffer");

        result = snd_pcm_hw_params_any(_pcm, parameters);
        ThrowIfError(result, "Unable to allocate parameters buffer");

        result = snd_pcm_hw_params_set_access(_pcm, parameters, PcmAccess.ReadWriteInterleaved);
        ThrowIfError(result, "Unable to set access");

        result = snd_pcm_hw_params_set_format(_pcm, parameters, PcmFormat.PcmFormatFloatLittleEndian);
        ThrowIfError(result, "Unable to set format");

        result = snd_pcm_hw_params_set_channels(_pcm, parameters, (uint)channelCount);
        ThrowIfError(result, "Unable to set channels");

        var rate = (uint)sampleRate;
        var dir = 0;

        result = snd_pcm_hw_params_set_rate_near(_pcm, parameters, ref rate, ref dir);
        ThrowIfError(result, "Unable to set sample rate");

        result = snd_pcm_hw_params_set_buffer_size_near(_pcm, parameters, ref bufferSize);
        ThrowIfError(result, "Unable to set buffer size");

        result = snd_pcm_hw_params_set_period_size_near(_pcm, parameters, ref periodSize, ref dir);
        ThrowIfError(result, "Unable to set period size");

        result = snd_pcm_hw_params(_pcm, parameters);
        ThrowIfError(result, "Unable to send Alsa params");

        //result = snd_pcm_hw_params_free(parameters);
        //ThrowIfError(result, "Unable to free parameters buffer");
    }

    private Thread CreateQueueWorkerThread() => new(QueueWorker)
    {
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal
    };

    private void QueueWorker()
    {
        _queueWorkerStopped = false;

        while (_isQueueRunning)
        {
            if (!_samplesQueue.Reader.TryRead(out var samples))
            {
                continue;
            }

            while (_isQueueRunning)
            {
                var audioData = new float[_audioData.Length];
                var audioDataLength = samples.ReadFrames(audioData, audioData.Length / _frameSize);
                if (audioDataLength == 0)
                {
                    break;
                }

                unsafe
                {
                    fixed (float* buffer = audioData)
                    {
                        var result = snd_pcm_writei(_pcm, (IntPtr)buffer, (ulong)audioDataLength);
                        if (result == -32)
                        {
                           // snd_pcm_prepare(_pcm);
                            snd_pcm_writei(_pcm, (IntPtr)buffer, (ulong)audioDataLength);
                        }
                    }
                }
            }
        }

        _queueWorkerStopped = true;
    }

    private static void ThrowIfError(int result, string message)
    {
        if (result >= 0)
        {
            return;
        }

        var alsaError = Marshal.PtrToStringAnsi(snd_strerror(result));
        throw new AudioPlayerException($"{message}: {alsaError}", result);
    }
}
