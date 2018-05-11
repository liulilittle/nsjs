namespace nsjsdotnet.Core.Hooking
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public unsafe abstract class NetHook
    {
        public abstract void Install(IntPtr oldMethodAddress, IntPtr newMethodAddress);

        public abstract void Suspend();

        public abstract void Resume();

        public abstract void Uninstall();

        public abstract IntPtr GetProcAddress(Delegate d);

        public abstract IntPtr GetProcAddress(string strLibraryName, string strMethodName);

        public static NetHook CreateNative() // ::IsWow64Process
        {
            if (IntPtr.Size != sizeof(int)) // Environment.Is64BitProcess
            {
                return new NetHook_Injection_Rax();
            }
            return new NetHook_Injection_Jmp();
        }

        public static NetHook CreateNative(IntPtr oldMethodAddress, IntPtr newMethodAddress)
        {
            NetHook hook = NetHook.CreateNative();
            try
            {
                return hook;
            }
            finally
            {
                hook.Install(oldMethodAddress, newMethodAddress);
            }
        }

        public static NetHook CreateNative(IntPtr oldMethodAddress, Delegate newMethodDelegate)
        {
            return NetHook.CreateNative(oldMethodAddress, Marshal.GetFunctionPointerForDelegate(newMethodDelegate));
        }

        public static NetHook CreateNative(IntPtr oldMethodAddress, MethodInfo newMethodInfo)
        {
            return CreateNative(oldMethodAddress, newMethodInfo.MethodHandle.GetFunctionPointer());
        }

        public static NetHook CreateManaged()
        {
            return new NetHook_Injection_Jmp();
        }

        public static NetHook CreateManaged(IntPtr oldMethodAddress, IntPtr newMethodAddress)
        {
            NetHook hook = NetHook.CreateManaged();
            try
            {
                return hook;
            }
            finally
            {
                hook.Install(oldMethodAddress, newMethodAddress);
            }
        }

        public static NetHook CreateManaged(IntPtr oldMethodAddress, Delegate newMethodDelegate)
        {
            return CreateManaged(oldMethodAddress, Marshal.GetFunctionPointerForDelegate(newMethodDelegate));
        }

        public static NetHook CreateManaged(IntPtr oldMethodAddress, MethodInfo newMethodInfo)
        {
            return CreateManaged(oldMethodAddress, newMethodInfo.MethodHandle.GetFunctionPointer());
        }

        public static NetHook CreateManaged(MethodInfo oldMethodInfo, MethodInfo newMethodInfo)
        {
            return CreateManaged(oldMethodInfo.MethodHandle.GetFunctionPointer(), newMethodInfo);
        }

        private abstract class NativeMethods
        {
            public const int PAGE_EXECUTE_READWRITE = 64;
            public static readonly IntPtr NULL = IntPtr.Zero;

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr LoadLibrary(string lpLibFileName);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, int flNewProtect, out int lpflOldProtect);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FreeLibrary([In] IntPtr hLibModule);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, IntPtr lpNumberOfBytesWritten);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetCurrentProcess();
        }

        private sealed class NetHook_Injection_Rax : NetHook // 48 B8 00 00 00 00 00 00 00 00 FF E0
        {
            private int mOldMemoryProtect;
            private IntPtr mOldMethodAddress;
            private IntPtr mNewMethodAddress;
            private byte[] mOldMethodAsmCode;
            private byte[] mNewMethodAsmCode;

            public override void Install(IntPtr oldMethodAddress, IntPtr newMethodAddress)
            {
                if (oldMethodAddress == NativeMethods.NULL || newMethodAddress == NativeMethods.NULL)
                    throw new Exception("The address is invalid.");
                if (!this.VirtualProtect(oldMethodAddress, NativeMethods.PAGE_EXECUTE_READWRITE))
                    throw new Exception("Unable to modify memory protection.");
                this.mOldMethodAddress = oldMethodAddress;
                this.mNewMethodAddress = newMethodAddress;
                this.mOldMethodAsmCode = this.GetHeadCode(this.mOldMethodAddress);
                this.mNewMethodAsmCode = this.ConvetToBinary((long)this.mNewMethodAddress);
                this.mNewMethodAsmCode = this.CombineOfArray(new byte[] { 0x48, 0xB8 }, this.mNewMethodAsmCode);
                this.mNewMethodAsmCode = this.CombineOfArray(this.mNewMethodAsmCode, new byte[] { 0xFF, 0xE0 });
                if (!this.WriteToMemory(this.mNewMethodAsmCode, this.mOldMethodAddress, 12))
                    throw new Exception("Cannot be written to memory.");
            }

            public override void Suspend()
            {
                if (this.mOldMethodAddress == NativeMethods.NULL)
                    throw new Exception("Unable to suspend.");
                this.WriteToMemory(this.mOldMethodAsmCode, this.mOldMethodAddress, 12);
            }

            public override void Resume()
            {
                if (this.mOldMethodAddress == NativeMethods.NULL)
                    throw new Exception("Unable to resume.");
                this.WriteToMemory(this.mNewMethodAsmCode, this.mOldMethodAddress, 12);
            }

            public override void Uninstall()
            {
                if (this.mOldMethodAddress == NativeMethods.NULL)
                    throw new Exception("Unable to uninstall.");
                if (!this.WriteToMemory(this.mOldMethodAsmCode, this.mOldMethodAddress, 12))
                    throw new Exception("Cannot be written to memory.");
                if (!this.VirtualProtect(this.mOldMethodAddress, this.mOldMemoryProtect))
                    throw new Exception("Unable to modify memory protection.");
                this.mOldMemoryProtect = 0;
                this.mOldMethodAsmCode = null;
                this.mNewMethodAsmCode = null;
                this.mOldMethodAddress = NativeMethods.NULL;
                this.mNewMethodAddress = NativeMethods.NULL;
            }

            private byte[] GetHeadCode(IntPtr ptr)
            {
                byte[] buffer = new byte[12];
                Marshal.Copy(ptr, buffer, 0, 12);
                return buffer;
            }

            private byte[] ConvetToBinary(long num)
            {
                byte[] buffer = new byte[8];
                IntPtr ptr = Marshal.AllocHGlobal(8);
                Marshal.WriteInt64(ptr, num);
                Marshal.Copy(ptr, buffer, 0, 8);
                Marshal.FreeHGlobal(ptr);
                return buffer;
            }

            private byte[] CombineOfArray(byte[] x, byte[] y)
            {
                int i = 0, len = x.Length;
                byte[] buffer = new byte[len + y.Length];
                while (i < len)
                {
                    buffer[i] = x[i];
                    i++;
                }
                while (i < buffer.Length)
                {
                    buffer[i] = y[i - len];
                    i++;
                }
                return buffer;
            }

            private bool WriteToMemory(byte[] buffer, IntPtr address, uint size)
            {
                IntPtr hRemoteProcess = NativeMethods.GetCurrentProcess();
                return NativeMethods.WriteProcessMemory(hRemoteProcess, address, buffer, size, NativeMethods.NULL);
            }

            public override IntPtr GetProcAddress(Delegate d)
            {
                return Marshal.GetFunctionPointerForDelegate(d);
            }

            public override IntPtr GetProcAddress(string strLibraryName, string strMethodName)
            {
                IntPtr hRemoteModule;
                if ((hRemoteModule = NativeMethods.GetModuleHandle(strLibraryName)) == NativeMethods.NULL)
                    hRemoteModule = NativeMethods.LoadLibrary(strLibraryName);
                return NativeMethods.GetProcAddress(hRemoteModule, strMethodName);
            }

            private bool VirtualProtect(IntPtr ptr, int flNewProtect)
            {
                return NativeMethods.VirtualProtect(ptr, 12, flNewProtect, out this.mOldMemoryProtect);
            }
        }

        private sealed class NetHook_Injection_Jmp : NetHook // E9 00 00 00 00
        {
            private int mOldMemoryProtect;
            private IntPtr mOldMethodAddress;
            private IntPtr mNewMethodAddress;
            private byte[] mOldMethodAsmCode;
            private byte[] mNewMethodAsmCode;

            public override unsafe void Install(IntPtr oldMethodAddress, IntPtr newMethodAddress)
            {
                if (oldMethodAddress == NativeMethods.NULL || newMethodAddress == NativeMethods.NULL)
                {
                    throw new Exception("The address is invalid.");
                }
                if (!this.VirtualProtect(oldMethodAddress, NativeMethods.PAGE_EXECUTE_READWRITE))
                {
                    throw new Exception("Unable to modify memory protection.");
                }
                this.mOldMethodAddress = oldMethodAddress;
                this.mNewMethodAddress = newMethodAddress;

                this.mOldMethodAsmCode = this.GetHeadCode(oldMethodAddress);

                this.mNewMethodAsmCode = new byte[5];
                this.mNewMethodAsmCode[0] = 0xE9;
                fixed (byte* p = &this.mNewMethodAsmCode[1])
                {
                    *(int*)p = (int)((long)newMethodAddress - ((long)oldMethodAddress + 5));
                }
                this.Resume();
            }

            public override void Suspend()
            {
                if (this.mOldMethodAddress == NativeMethods.NULL)
                {
                    throw new Exception("Unable to suspend.");
                }
                this.WriteToMemory(this.mOldMethodAsmCode, this.mOldMethodAddress, 5);
            }

            public override void Resume()
            {
                if (this.mOldMethodAddress == NativeMethods.NULL)
                {
                    throw new Exception("Unable to resume.");
                }
                this.WriteToMemory(this.mNewMethodAsmCode, this.mOldMethodAddress, 5);
            }

            public override void Uninstall()
            {
                if (this.mOldMethodAddress == NativeMethods.NULL)
                {
                    throw new Exception("Unable to uninstall.");
                }
                this.Resume();
                if (!this.VirtualProtect(this.mOldMethodAddress, this.mOldMemoryProtect))
                {
                    throw new Exception("Unable to modify memory protection.");
                }
                this.mOldMemoryProtect = 0;
                this.mOldMethodAsmCode = null;
                this.mNewMethodAsmCode = null;
                this.mOldMethodAddress = NativeMethods.NULL;
                this.mNewMethodAddress = NativeMethods.NULL;
            }

            private unsafe byte[] GetHeadCode(IntPtr ptr)
            {
                byte[] buffer = new byte[5];
                fixed (byte* pinned = buffer)
                {
                    byte* src = (byte*)ptr;
                    byte* dest = (byte*)pinned;
                    for (int i = 0; i < 5; i++)
                    {
                        *dest++ = *src++;
                    }
                }
                return buffer;
            }

            private byte[] ConvetToBinary(int num)
            {
                byte[] buffer = new byte[4];
                fixed (byte* p = buffer)
                {
                    *(int*)p = num;
                }
                return buffer;
            }

            private bool WriteToMemory(byte[] buffer, IntPtr address, uint size)
            {
                fixed (byte* pinned = buffer)
                {
                    byte* src = pinned;
                    byte* dest = (byte*)address;
                    for (int i = 0; i < size; i++)
                    {
                        *dest++ = *src++;
                    }
                }
                return true;
            }

            public override IntPtr GetProcAddress(Delegate d)
            {
                return Marshal.GetFunctionPointerForDelegate(d);
            }

            public override IntPtr GetProcAddress(string strLibraryName, string strMethodName)
            {
                IntPtr hRemoteModule;
                if ((hRemoteModule = NativeMethods.GetModuleHandle(strLibraryName)) == NativeMethods.NULL)
                {
                    hRemoteModule = NativeMethods.LoadLibrary(strLibraryName);
                }
                return NativeMethods.GetProcAddress(hRemoteModule, strMethodName);
            }

            private bool VirtualProtect(IntPtr ptr, int flNewProtect)
            {
                return NativeMethods.VirtualProtect(ptr, 5, flNewProtect, out this.mOldMemoryProtect);
            }
        }
    }
}
