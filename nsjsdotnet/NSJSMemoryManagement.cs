namespace nsjsdotnet
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public unsafe static class NSJSMemoryManagement
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static byte* nsjs_memory_alloc(uint size);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_memory_free(void* memory);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static bool IsBadReadPtr(void* lp, uint ucb);

        [DllImport("kernel32.dll", SetLastError = false)]
        private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [DllImport("nsjs.dll", SetLastError = false)]
        private static extern int nsjs_idlelocalvalues_getcapacity();

        [DllImport("nsjs.dll", SetLastError = false)]
        private static extern void nsjs_idlelocalvalues_setcapacity(int capacity);

        [DllImport("nsjs.dll", SetLastError = false)]
        private static extern int nsjs_activelocalvalues_getcount();

        public static int IdleValueCapacity
        {
            get
            {
                return nsjs_idlelocalvalues_getcapacity();
            }
            set
            {
                nsjs_idlelocalvalues_setcapacity(value);
            }
        }

        public static int ActiveValueCount
        {
            get
            {
                return nsjs_activelocalvalues_getcount();
            }
        }

        public static void Optimization()
        {
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }

        public static void Release()
        {
            Release(false);
        }

        public static void ReleaseAndOptimization(bool forced)
        {
            Release(forced);
            Optimization();
        }

        public static void ReleaseAndOptimization()
        {
            ReleaseAndOptimization(false);
        }

        public static void Release(bool forced)
        {
            if (!forced)
            {
                GC.Collect();
            }
            else
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
            GC.WaitForPendingFinalizers();
        }

        public static byte* Alloc(uint size)
        {
            if (size <= 0)
            {
                return null;
            }
            return nsjs_memory_alloc(size);
        }

        public static void Free(IntPtr memory)
        {
            NSJSMemoryManagement.Free(memory.ToPointer());
        }

        public static void Free(void* memory)
        {
            if (memory != null && !IsBadReadPtr(memory, 0x01))
            {
                nsjs_memory_free(memory);
            }
        }
    }
}
