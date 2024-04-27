using System.Runtime.InteropServices;

namespace OldBit.Beeper.Windows.CoreAudioInterop
{
    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
    }
}
