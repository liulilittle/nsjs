namespace nsjsdotnet.Core.Utilits
{
    using System;

    public class Platform
    {
        public static bool IsWindowsNT()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT;
        }

        public static bool IsWin32Windows()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32Windows;
        }

        public static bool IsWin32S()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32S;
        }

        public static bool IsWinCE()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.WinCE;
        }

        public static bool IsWindows()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT ||
                os.Platform == PlatformID.Win32Windows ||
                os.Platform == PlatformID.Win32S ||
                os.Platform == PlatformID.WinCE;
        }

        public static bool IsUnix()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Unix;
        }

        public static bool IsXbox()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Xbox;
        }

        public static bool IsMacOSX()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.MacOSX;
        }

        public static bool IsIA32os()
        {
            return !Environment.Is64BitOperatingSystem;
        }

        public static bool IsIA64os()
        {
            return Environment.Is64BitOperatingSystem;
        }

        public static bool IsIA64Process()
        {
            return Environment.Is64BitProcess;
        }

        public static bool IsIA32Process()
        {
            return !Environment.Is64BitProcess;
        }
    }
}
