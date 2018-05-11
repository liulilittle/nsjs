namespace nsjsdotnet.Core.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class Waitable
    {
        private abstract class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = false)]
            public static extern IntPtr CreateWaitableTimer(int lpTimerAttributes, bool bManualReset, int lpTimerName);
            [DllImport("kernel32.dll", SetLastError = false)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetWaitableTimer(IntPtr hTimer, ref long pDueTime, int lPeriod, int pfnCompletionRoutine, int lpArgToCompletionRoutine, bool fResume);
            [DllImport("user32.dll", SetLastError = false)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MsgWaitForMultipleObjects(uint nCount, ref IntPtr pHandles, bool bWaitAll, int dwMilliseconds, uint dwWakeMask);
            [DllImport("kernel32.dll", SetLastError = false)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hWnd);
            public const int NULL = 0;
            public const int QS_TIMER = 0x10;
        }

        public static void usleep(int us)
        {
            Waitable.nanosleep(us * 10);
        }

        public static void sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        public static void nanosleep(int ns100)
        {
            if (ns100 < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            long duetime = -1 * ns100;
            IntPtr hWaitTimer = NativeMethods.CreateWaitableTimer(NativeMethods.NULL, true, NativeMethods.NULL);
            NativeMethods.SetWaitableTimer(hWaitTimer, ref duetime, 0, NativeMethods.NULL, NativeMethods.NULL, false);
            while (NativeMethods.MsgWaitForMultipleObjects(1, ref hWaitTimer, false, Timeout.Infinite, NativeMethods.QS_TIMER)) ;
            NativeMethods.CloseHandle(hWaitTimer);
        }
    }
}
