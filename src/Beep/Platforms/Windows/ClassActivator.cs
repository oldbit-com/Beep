using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace OldBit.Beep.Platforms.Windows;

internal static partial class ClassActivator
{
    private const int ClsCtxInprocServer = 0x01;

    private static readonly ComWrappers ComWrappers = new StrategyBasedComWrappers();

    internal static T Activate<T>(string clsid, string iid) => Activate<T>(new Guid(clsid), new Guid(iid));

    private static T Activate<T>(Guid clsid, Guid iid)
    {
        var result = CoCreateInstance(ref clsid, IntPtr.Zero, ClsCtxInprocServer, ref iid, out var obj);

        if (result < 0)
        {
            Marshal.ThrowExceptionForHR(result);
        }

        return (T)ComWrappers.GetOrCreateObjectForComInstance(obj, CreateObjectFlags.None);
    }

    [LibraryImport("ole32")]
    private static partial int CoCreateInstance(
        ref Guid rclsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        ref Guid riid,
        out IntPtr ppObj);
}