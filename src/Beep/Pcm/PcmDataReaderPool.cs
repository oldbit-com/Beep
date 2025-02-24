using OldBit.Beep.Filters;

namespace OldBit.Beep.Pcm;

/// <summary>
/// A pool of <see cref="PcmDataReader"/> instances. This is used to avoid creating new instances and
/// minimize the garbage collection overhead.
/// </summary>
internal sealed class PcmDataReaderPool
{
    private readonly List<PcmDataReader> _pool = [];
    private int _index;

    public PcmDataReaderPool(int capacity, AudioFormat audioFormat, VolumeFilter volumeFilter)
    {
        for (var i = 0; i < capacity * 2; i++)
        {
            _pool.Add(new PcmDataReader(audioFormat, volumeFilter));
        }
    }

    internal PcmDataReader GetReader(byte[] data, int count)
    {
        var reader = _pool[_index];
        reader.SetData(data, count);

        _index = (_index + 1) % _pool.Count;

        return reader;
    }
}