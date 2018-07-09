namespace nsjsdotnet.Core.Utilits
{
    using System;
    using System.Management;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Hardware
    {
        [DllImport("kernel32.dll", SetLastError = false, CharSet = CharSet.Ansi)]
        private static extern int GetLogicalDriveStrings(int nBufferLength, StringBuilder lpBuffer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool GetVolumeInformation(
            string rootPathName,
            StringBuilder volumeNameBuffer,
            int volumeNameSize,
            out uint volumeSerialNumber,
            out uint maximumComponentLength,
            uint fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            int nFileSystemNameSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = false, CharSet = CharSet.Ansi)]
        private delegate int __cpuid(ref int s1, ref int s2);

        private static string m_absoluteRealHardwareCode;
        private static string m_hdId;
        private static string m_cpuid;
        private static readonly object m_syncobj = new object();

        public static string CPUID
        {
            get
            {
                lock (m_syncobj)
                {
                    if (m_cpuid == null)
                    {
                        byte[] shellcode = null;
                        if (!Environment.Is64BitProcess) // Instant ready to fetch cpuid shellcode and execute immediately
                        {
                            /*
                                pushad // storage register environment
		                        mov eax, 01h
		                        xor ecx, ecx
		                        xor edx, edx
		                        cpuid
		                        mov ecx, dword ptr[ebp+8]
		                        mov dword ptr[ecx], edx
		                        mov ecx, dword ptr[ebp+0Ch]
		                        mov dword ptr[ecx], eax
		                        popad // restore register environment
                            */
                            shellcode = new byte[] { 96, 184, 1, 0, 0, 0, 51, 201, 51, 210, 15, 162, 139, 77, 8, 137, 17, 139, 77, 12, 137, 1, 97, 195 };
                        }
                        else
                        {
                            /*
                                 mov         dword ptr [rsp+10h],edx         
                                 mov         dword ptr [rsp+8],ecx  
                                 push        rbp  
                                 push        rdi  
                                 sub         rsp,0C8h  
                                 mov         rbp,rsp  
                                 mov         rdi,rsp  
                                 mov         ecx,32h  
                                 mov         eax,0CCCCCCCCh  
                                 rep stos    dword ptr [rdi]  
                                 mov         eax, 01h
                                 xor         ecx, ecx
                                 xor         edx, edx
                                 cpuid
                                 mov         ecx,dword ptr [rbp+00000000000000E0h] 
                                 mov         dword ptr[ecx], edx
                                 mov         ecx,dword ptr [rbp+00000000000000E8h]
                                 mov         dword ptr[ecx], eax
                                 lea         rsp,[rbp+00000000000000C8h]  
                                 pop         rdi  
                                 pop         rbp  
                                 ret
                            */
                            shellcode = new byte[]
                            {
                                137,84,36,16,137,76,36,8,85,87,72,129,236,200,0,0,0,72,139,236,72,139,252,185,
                                50,0,0,0,184,204,204,204,204,243,171,184,1,0,0,0,51,201,51,210,15,162,139,141,
                                224,0,0,0,103,137,17,139,141,232,0,0,0,103,137,1,72,141,165,200,0,0,0,95,93,195,
                            };
                        }
                        GCHandle handle = GCHandle.Alloc(shellcode, GCHandleType.Pinned);
                        if (!NSJSFunction.MarkShellcode(shellcode, 0, shellcode.Length))
                        {
                            throw new InvalidOperationException("Unable to mark shellcode");
                        }
                        __cpuid cpuid = (__cpuid)Marshal.GetDelegateForFunctionPointer(handle.AddrOfPinnedObject(), typeof(__cpuid));
                        try
                        {
                            int s1 = 0;
                            int s2 = 0;
                            cpuid(ref s1, ref s2);
                            m_cpuid = s1.ToString("X2") + s2.ToString("X2");
                        }
                        finally
                        {
                            handle.Free();
                        }
                        /* The following note is part of the WMI-mode fetch cpuid, but it's very inefficient
                            using (ManagementClass mc = new ManagementClass("Win32_Processor"))
                            {
                                ManagementObjectCollection moc = mc.GetInstances();
                                foreach (ManagementObject mo in moc)
                                {
                                    m_cpuid = mo.Properties["ProcessorId"].Value.ToString();
                                }
                            }
                        */
                    }
                    if (m_cpuid == null)
                    {
                        throw new InvalidOperationException("No available CPU info");
                    }
                    return m_cpuid;
                }
            }
        }

        public static string HDID
        {
            get
            {
                lock (m_syncobj)
                {
                    if (m_hdId == null)
                    {
                        using (ManagementClass mc = new ManagementClass("Win32_DiskDrive"))
                        {
                            ManagementObjectCollection moc = mc.GetInstances();
                            foreach (ManagementObject mo in moc)
                            {
                                return (m_hdId = (string)mo.Properties["Model"].Value);
                            }
                        }
                        throw new InvalidOperationException("No available Disk info");
                    }
                    return m_hdId;
                }
            }
        }

        public static string MACID
        {
            get
            {
                using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    ManagementObjectCollection moc = mc.GetInstances();
                    foreach (ManagementObject mo in moc)
                    {
                        if ((bool)mo["IPEnabled"] == true)
                        {
                            return mo["MacAddress"].ToString();
                        }
                    }
                }
                throw new InvalidOperationException("No available NIC found");
            }
        }

        public static string PrimaryPartitionDiskId
        {
            get
            {
                lock (m_syncobj)
                {
                    if (m_absoluteRealHardwareCode == null)
                    {
                        int dw = GetLogicalDriveStrings(0, null);
                        if (dw <= 0)
                        {
                            return null;
                        }
                        StringBuilder buffer = new StringBuilder(dw);
                        GetLogicalDriveStrings(dw, buffer);
                        uint serials, maxcomp;
                        if (!GetVolumeInformation(buffer.ToString(), null, 0, out serials, out maxcomp, 0, null, 0))
                        {
                            return null;
                        }
                        m_absoluteRealHardwareCode = serials.ToString("X2");
                    }
                    return m_absoluteRealHardwareCode;
                }
            }
        }
    }
}
