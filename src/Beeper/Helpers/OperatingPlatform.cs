using System.Runtime.InteropServices;

namespace OldBit.Beeper.Helpers;

internal static class OperatingPlatform
{
    internal static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    internal static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}