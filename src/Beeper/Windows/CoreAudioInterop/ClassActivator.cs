using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OldBit.Beeper.Windows.CoreAudioInterop;

internal static class ClassActivator
{
    private const int ClsCtxInprocServer = 0x01;

    private static readonly ComWrappers _comWrappers = new StrategyBasedComWrappers();

    internal static I Activate<I>(string clsid, string iid) => Activate<I>(new Guid(clsid), new Guid(iid));

    private static I Activate<I>(Guid clsid, Guid iid)
    {
        var result = CoCreateInstance(ref clsid, IntPtr.Zero, ClsCtxInprocServer, ref iid, out var obj);
        
        if (result < 0)
        {
            Marshal.ThrowExceptionForHR(result);
        }

        return (I)_comWrappers.GetOrCreateObjectForComInstance(obj, CreateObjectFlags.None);
    }

    [DllImport("ole32")]
    private static extern int CoCreateInstance(
        ref Guid rclsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        ref Guid riid,
        out IntPtr ppObj);
}