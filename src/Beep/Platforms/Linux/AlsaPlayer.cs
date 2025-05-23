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

    private readonly Channel<PcmDataReader> _samplesQueue;
    private readonly Thread _queueWorker;
    private readonly float[] _audioData;
    private readonly int _channelCount;

    private IntPtr _pcm = IntPtr.Zero;
    private IntPtr _parameters = IntPtr.Zero;
    private bool _isQueueRunning;
    private bool _queueWorkerStopped = true;

    internal AlsaPlayer(int sampleRate, int channelCount, PlayerOptions playerOptions)
    {
        _channelCount = channelCount;
        var frameSize = channelCount * FloatType.SizeInBytes;

        _samplesQueue = Channel.CreateBounded<PcmDataReader>(new BoundedChannelOptions(playerOptions.BufferQueueSize)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var periodSize = playerOptions.BufferSizeInBytes / (frameSize * Period);
        var bufferSize = periodSize * Period;

        try
        {
            Initialize(sampleRate, channelCount, (ulong)periodSize, (ulong)bufferSize);
        }
        catch (DllNotFoundException ex)
        {
            throw new AudioPlayerException("Make sure ALSA development library has been installed.", ex);
        }

        _audioData = new float[periodSize * channelCount];

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

        Close();
    }

    public event EventHandler<PlayerErrorEventArgs>? OnError;

    public void Dispose() => Close();

    private void Initialize(int sampleRate, int channelCount, ulong periodSize, ulong bufferSize)
    {
        var result = snd_pcm_open(ref _pcm, "default", PcmStream.Playback, 0);
        ThrowIfError(result, "Unable to open PCM");

        result = snd_pcm_hw_params_malloc(ref _parameters);
        ThrowIfError(result, "Unable to allocate params buffer");

        result = snd_pcm_hw_params_any(_pcm, _parameters);
        ThrowIfError(result, "Unable to fill params buffer");

        result = snd_pcm_hw_params_set_access(_pcm, _parameters, PcmAccess.ReadWriteInterleaved);
        ThrowIfError(result, "Unable to set access");

        result = snd_pcm_hw_params_set_format(_pcm, _parameters, PcmFormat.FloatLittleEndian);
        ThrowIfError(result, "Unable to set format");

        result = snd_pcm_hw_params_set_channels(_pcm, _parameters, (uint)channelCount);
        ThrowIfError(result, "Unable to set channels");

        var rate = (uint)sampleRate;
        var dir = 0;

        result = snd_pcm_hw_params_set_rate_near(_pcm, _parameters, ref rate, ref dir);
        ThrowIfError(result, "Unable to set sample rate");

        result = snd_pcm_hw_params_set_buffer_size_near(_pcm, _parameters, ref bufferSize);
        ThrowIfError(result, "Unable to set buffer size");

        result = snd_pcm_hw_params_set_period_size_near(_pcm, _parameters, ref periodSize, ref dir);
        ThrowIfError(result, "Unable to set period size");

        result = snd_pcm_hw_params(_pcm, _parameters);
        ThrowIfError(result, "Unable to send params");

        snd_pcm_hw_params_free(_parameters);
    }

    private Thread CreateQueueWorkerThread() => new(() =>
    {
        try
        {
            RunWorker();
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, new PlayerErrorEventArgs(ex));
        }
    })
    {
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal
    };

    private void Close()
    {
        if (_pcm == IntPtr.Zero)
        {
            return;
        }

        var result = snd_pcm_close(_pcm);
        ThrowIfError(result, "Unable to close PCM handle");

        _pcm = IntPtr.Zero;
    }

    private void RunWorker()
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
                var audioDataLength = samples.ReadSamples(_audioData, _audioData.Length);

                if (audioDataLength == 0)
                {
                    break;
                }

                var result = snd_pcm_writei(_pcm, _audioData, (ulong)(audioDataLength / _channelCount));

                if (result == -32)
                {
                    result = snd_pcm_recover(_pcm, result, 0);
                    ThrowIfError(result, "Unable to recover from buffer underrun");
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
