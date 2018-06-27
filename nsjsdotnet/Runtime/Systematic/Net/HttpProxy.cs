namespace nsjsdotnet.Runtime.Systematic.Net
{
    using Microsoft.Win32;
    using global::System;
    using global::System.Diagnostics;
    using global::System.Net;
    using global::System.Reflection;
    using global::System.Runtime.InteropServices;
    using global::System.Text.RegularExpressions;
    using HTTPWebRequest = global::System.Net.HttpWebRequest;

    unsafe static class HttpProxy
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static HttpProxy()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("Update", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Update));
            owner.Set("SetProxy", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetProxy));
            owner.Set("GetSystemProxyAddress", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetSystemProxyAddress));
        }

        private static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct INTERNET_PROXY_INFO
            {
                public int dwAccessType;
                public void* proxy;
                public void* proxyBypass;
            };

            [DllImport("wininet.dll", EntryPoint = "InternetSetOptionA", CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

            [DllImport("kernel32.dll", SetLastError = false)]
            public static extern void* RtlZeroMemory(void* src, int size);

            public const int INTERNET_OPTION_SETTINGS_CHANGED = 39; // 修改设置完成
            public const int INTERNET_OPTION_REFRESH = 37; // 刷新网路设置
            public const int INTERNET_OPTION_PROXY = 38; // 代理服务器设置
            public const int INTERNET_OPTION_PROXY_SETTINGS_CHANGED = 95;

            public const int INTERNET_OPEN_TYPE_PROXY = 3;
        }

        private static RegistryKey Configuration
        {
            get
            {
                return Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);
            }
        }

        public static bool Update(string server)
        {
            NativeMethods.INTERNET_PROXY_INFO* ipi = (NativeMethods.INTERNET_PROXY_INFO*)Marshal.AllocHGlobal(sizeof(NativeMethods.INTERNET_PROXY_INFO));
            ipi->dwAccessType = NativeMethods.INTERNET_OPEN_TYPE_PROXY;
            ipi->proxy = (void*)Marshal.StringToHGlobalAuto(server ?? string.Empty);
            ipi->proxyBypass = (void*)Marshal.StringToHGlobalAuto("local");
            bool retval = NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_PROXY, (IntPtr)ipi, sizeof(NativeMethods.INTERNET_PROXY_INFO));
            Marshal.FreeHGlobal((IntPtr)ipi->proxy);
            Marshal.FreeHGlobal((IntPtr)ipi->proxyBypass);
            Marshal.FreeHGlobal((IntPtr)ipi);
            return retval;
        }

        public static bool Update()
        {
            NativeMethods.INTERNET_PROXY_INFO* ipi = (NativeMethods.INTERNET_PROXY_INFO*)Marshal.AllocHGlobal(sizeof(NativeMethods.INTERNET_PROXY_INFO));
            NativeMethods.RtlZeroMemory(ipi, sizeof(NativeMethods.INTERNET_PROXY_INFO));
            bool retval =
                NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_PROXY_SETTINGS_CHANGED, IntPtr.Zero, 0) &&
                NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_SETTINGS_CHANGED, (IntPtr)ipi, sizeof(NativeMethods.INTERNET_PROXY_INFO)) &&
                NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            Marshal.FreeHGlobal((IntPtr)ipi);
            return retval;
        }

        private static bool RegistrySetValue(RegistryKey registry, string key, object value)
        {
            if (registry != null && !string.IsNullOrEmpty(key))
            {
                try
                {
                    registry.SetValue(key, value);
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        private static T RegistryGetValue<T>(RegistryKey registry, string key)
        {
            object obj = null;
            if (registry != null && !string.IsNullOrEmpty(key))
            {
                try
                {
                    obj = registry.GetValue(key, null);
                }
                catch { }
            }
            if (obj != null)
            {
                return (T)obj;
            }
            return default(T);
        }

        public static bool SetProxy(string server, string pac, bool enabled)
        {
            try
            {
                using (RegistryKey registry = HttpProxy.Configuration)
                {
                    RegistrySetValue(registry, "ProxyServer", server);
                    RegistrySetValue(registry, "ProxyEnable", (enabled ? 1 : 0));
                    RegistrySetValue(registry, "AutoConfigURL", pac);
                    HttpProxy.Update(server);
                    RegistrySetValue(registry, "ProxyOverride", "localhost;127.*;10.*;172.16.*;172.17.*;172.18.*;172.19.*;172.20.*;172.21.*;172.22.*;172.23.*;172.24.*;172.25.*;172.26.*;172.27.*;172.28.*;172.29.*;172.30.*;172.31.*;172.32.*;192.168.*;<local>");
                    HttpProxy.Update();
                    if (!Regex.IsMatch(RegistryGetValue<string>(registry, "ProxyServer") ?? string.Empty, server))
                    {
                        RegistrySetValue(registry, "ProxyServer", server);
                        RegistrySetValue(registry, "ProxyEnable", (enabled ? 1 : 0));
                        RegistrySetValue(registry, "AutoConfigURL", pac);
                        HttpProxy.Update(server);
                        RegistrySetValue(registry, "ProxyOverride", "localhost;127.*;10.*;172.16.*;172.17.*;172.18.*;172.19.*;172.20.*;172.21.*;172.22.*;172.23.*;172.24.*;172.25.*;172.26.*;172.27.*;172.28.*;172.29.*;172.30.*;172.31.*;172.32.*;192.168.*;<local>");
                        // control.exe inetcpl.cpl
                        Process.Start("rundll32", "shell32.dll,Control_RunDLL inetcpl.cpl");
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetSystemProxyAddress()
        {
            IWebProxy swp = HTTPWebRequest.GetSystemWebProxy();
            if (swp == null)
            {
                return string.Empty;
            }
            WebProxy proxy = swp.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)[0].GetValue(swp) as WebProxy;
            if (proxy == null)
            {
                return string.Empty;
            }
            Uri address = proxy.Address;
            if (address == null)
            {
                return string.Empty;
            }
            return address.ToString();
        }

        private static void GetSystemProxyAddress(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(GetSystemProxyAddress());
        }

        private static void Update(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(Update());
        }

        private static void SetProxy(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string server = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            string pac = arguments.Length > 1 ? (arguments[1] as NSJSString)?.Value : null;
            bool enabled = arguments.Length > 2 ? ((arguments[2] as NSJSBoolean)?.Value).GetValueOrDefault() : false;
            arguments.SetReturnValue(SetProxy(server, pac, enabled));
        }
    }
}
