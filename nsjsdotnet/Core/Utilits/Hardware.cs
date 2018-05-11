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
                        using (ManagementClass mc = new ManagementClass("Win32_Processor"))
                        {
                            ManagementObjectCollection moc = mc.GetInstances();
                            foreach (ManagementObject mo in moc)
                            {
                                return (m_cpuid = mo.Properties["ProcessorId"].Value.ToString());
                            }
                        }
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
