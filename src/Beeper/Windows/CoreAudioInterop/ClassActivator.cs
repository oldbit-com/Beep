using System.Runtime.InteropServices;

namespace OldBit.Beeper.Windows.CoreAudioInterop
{
    internal static class ClassActivator
    {
        public static I Activate<I>(string clsid, string iid) => Activate<I>(new Guid(clsid), new Guid(iid));

        public static I Activate<I>(Guid clsid, Guid iid)
        {
            var result = CoCreateInstance(ref clsid, IntPtr.Zero, /*CLSCTX_INPROC_SERVER*/ 1, ref iid, out object obj);
            if (result < 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            return (I)obj;
        }

        [DllImport("ole32")]
        private static extern int CoCreateInstance(
            ref Guid rclsid,
            IntPtr pUnkOuter, 
            uint dwClsContext, 
            ref Guid riid, 
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObj);

    }
}
