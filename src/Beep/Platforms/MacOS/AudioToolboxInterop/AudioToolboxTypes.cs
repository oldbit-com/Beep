using System.Runtime.InteropServices;

namespace OldBit.Beep.Platforms.MacOS.AudioToolboxInterop;

[StructLayout(LayoutKind.Sequential)]
internal struct AudioStreamBasicDescription
{
    internal double SampleRate;
    internal AudioFormatType Format;
    internal AudioFormatFlags FormatFlags;
    internal uint BytesPerPacket;
    internal uint FramesPerPacket;
    internal uint BytesPerFrame;
    internal uint ChannelsPerFrame;
    internal uint BitsPerChannel;
    internal uint Reserved;
}

internal enum AudioFormatType : uint
{
    LinearPcm = 0x6c70636d,
}

[Flags]
internal enum AudioFormatFlags : uint
{
    AudioFormatFlagIsFloat = 1,
}

[StructLayout(LayoutKind.Sequential)]
internal struct AudioTimeStamp
{
    public double SampleTime;
    public uint HostTime;
    public double RateScalar;
    public uint WordClockTime;
    public uint SMPTETime;
    public uint Flags;
    public uint Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct AudioQueueBuffer
{
    public uint AudioDataBytesCapacity;
    public nint AudioData;
    public uint AudioDataByteSize;
    public nint UserData;
    public uint PacketDescriptionCapacity;
    public nint PacketDescriptions;
    public uint PacketDescriptionCount;
}

[StructLayout(LayoutKind.Sequential)]
internal struct AudioStreamPacketDescription
{
    public long StartOffset;
    public uint VariableFramesInPacket;
    public uint DataByteSize;
}