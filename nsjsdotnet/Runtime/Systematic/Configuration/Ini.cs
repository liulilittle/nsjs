namespace nsjsdotnet.Runtime.Systematic.Configuration
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.IO;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.IO;
    using global::System.Runtime.InteropServices;
    using global::System.Text;

    static class Ini
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = false)]
        private static extern int GetPrivateProfileSectionNames([Out, MarshalAs(UnmanagedType.LPArray)] char[] lpszReturnBuffer, int nSize, [In, MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = false)]
        private static extern int GetPrivateProfileSection(string lpAppName, [MarshalAs(UnmanagedType.VBByRefStr)]ref string lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = false)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string files);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        private const int MAXPATH = 260;

        public static IEnumerable<string> GetAllSection(string path)
        {
            List<string> sections = new List<string>();
            int len = FileAuxiliary.GetFileLength(path);
            if (len > 0)
            {
                char[] buffer = new char[len];
                if ((len = GetPrivateProfileSectionNames(buffer, len, path)) > 0)
                {
                    unsafe
                    {
                        fixed (char* str = buffer)
                        {
                            char* ptr = str; int index = 0, last = 0;
                            while (index < len)
                            {
                                if (*ptr++ == '\0')
                                {
                                    int size = index - last;
                                    char[] name = new char[size];
                                    global::System.Array.Copy(buffer, last, name, 0, size);
                                    sections.Add(new string(name));
                                    last = index + 1;
                                }
                                index++;
                            }
                        }
                    }
                }
            }
            return sections;
        }

        public static IEnumerable<string> GetAllKey(string path, string section)
        {
            List<string> sections = new List<string>();
            foreach (KeyValuePair<string, string> pair in GetAllKeyValue(path, section))
            {
                sections.Add(pair.Key);
            }
            return sections;
        }

        public static IEnumerable<string> GetAllValue(string path, string section)
        {
            List<string> sections = new List<string>();
            foreach (KeyValuePair<string, string> pair in GetAllKeyValue(path, section))
            {
                sections.Add(pair.Value);
            }
            return sections;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetAllKeyValue(string path, string section)
        {
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
            int len = FileAuxiliary.GetFileLength(path);
            if (len > 0)
            {
                string buffer = new string('\0', len);
                if (GetPrivateProfileSection(section, ref buffer, len, path) > 0)
                {
                    string[] items = buffer.Split('\0');
                    foreach (string str in items)
                    {
                        int i;
                        if (str.Length > 0 && (i = str.IndexOf('=')) > -1)
                        {
                            keyValues.Add(new KeyValuePair<string, string>(str.Substring(0, i), str.Substring(i + 1)));
                        }
                    }
                }
            }
            return keyValues;
        }

        public static string GetKeyValue(string path, string section, string key)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
            {
                return null;
            }
            int size = MAXPATH;
            while (true)
            {
                if (!File.Exists(path))
                {
                    return null;
                }
                StringBuilder sb = new StringBuilder(size);
                uint count = GetPrivateProfileString(section, key, string.Empty, sb, sb.Capacity, path);
                if ((sb.Capacity - 1) == count)
                {
                    size *= 4;
                    continue;
                }
                return sb.ToString();
            }
        }

        public static bool SetKeyValue(string path, string section, string key, string value)
        {
            return WritePrivateProfileString(section, key, value, path);
        }

        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static Ini()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("GetKeyValue", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetKeyValue));
            owner.Set("SetKeyValue", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetKeyValue));
            owner.Set("GetAllValue", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetAllValue));
            owner.Set("GetAllKey", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetAllKey));
            owner.Set("GetAllSection", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetAllSection));
            owner.Set("GetAllKeyValue", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetAllKeyValue));
            owner.Set("GetConfigurationView", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetConfigurationView));
        }

        private static string GetFullPath(string path)
        {
            if (path == null)
            {
                return null;
            }
            return Path.GetFullPath(path);
        }

        private static void GetConfigurationView(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string path = GetFullPath(arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null);
            if (File.Exists(path))
            {
                NSJSObject configuration = NSJSObject.New(arguments.VirtualMachine);
                foreach (string section in GetAllSection(path))
                {
                    if (string.IsNullOrEmpty(section))
                    {
                        continue;
                    }
                    NSJSObject sectionObject = NSJSObject.New(arguments.VirtualMachine);
                    foreach (KeyValuePair<string, string> kv in GetAllKeyValue(path, section))
                    {
                        if (string.IsNullOrEmpty(kv.Key))
                        {
                            continue;
                        }
                        sectionObject.Set(kv.Key, kv.Value);
                    }
                    configuration.Set(section, sectionObject);
                }
                arguments.SetReturnValue(configuration);
            }
        }

        private static void GetAllKeyValue(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue result = null;
            if (arguments.Length > 0)
            {
                string path = GetFullPath((arguments[0] as NSJSString)?.Value);
                string section = (arguments[1] as NSJSString)?.Value;
                if (!string.IsNullOrEmpty(path) &&
                    !string.IsNullOrEmpty(section))
                {
                    IEnumerable<KeyValuePair<string, string>> s = GetAllKeyValue(path, section);
                    result = ArrayAuxiliary.ToArray(arguments.VirtualMachine, s);
                }
            }
            arguments.SetReturnValue(NSJSValue.UndefinedMerge(arguments.VirtualMachine, result));
        }

        private static void GetAllSection(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue result = null;
            if (arguments.Length > 0)
            {
                string path = GetFullPath((arguments[0] as NSJSString)?.Value);
                if (File.Exists(path))
                {
                    result = ArrayAuxiliary.ToArray(arguments.VirtualMachine, GetAllSection(path));
                }
            }
            arguments.SetReturnValue(NSJSValue.UndefinedMerge(arguments.VirtualMachine, result));
        }

        private static void GetAllKey(IntPtr info)
        {
            GetAllKeyOrValue(info, false);
        }
        
        private static void GetAllValue(IntPtr info)
        {
            GetAllKeyOrValue(info, true);
        }

        private static void GetAllKeyOrValue(IntPtr info, bool allValue)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue result = null;
            if (arguments.Length > 0)
            {
                string path = GetFullPath((arguments[0] as NSJSString)?.Value);
                string section = (arguments[1] as NSJSString)?.Value;
                if (!string.IsNullOrEmpty(path) &&
                    !string.IsNullOrEmpty(section))
                {
                    IEnumerable<string> s = (allValue ? GetAllValue(path, section) : GetAllKey(path, section));
                    result = ArrayAuxiliary.ToArray(arguments.VirtualMachine, s);
                }
            }
            arguments.SetReturnValue(NSJSValue.UndefinedMerge(arguments.VirtualMachine, result));
        }

        private static void GetKeyValue(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string value = null;
            if (arguments.Length > 2)
            {
                string path = GetFullPath((arguments[0] as NSJSString)?.Value);
                string section = (arguments[1] as NSJSString)?.Value;
                string key = (arguments[2] as NSJSString)?.Value;
                if (File.Exists(path) &&
                    !string.IsNullOrEmpty(section) &&
                    !string.IsNullOrEmpty(key))
                {
                    value = GetKeyValue(path, section, key);
                }
            }
            if (value != null)
            {
                arguments.SetReturnValue(value);
            }
            else
            {
                arguments.SetReturnValue(NSJSValue.Undefined(arguments.VirtualMachine));
            }
        }

        private static void SetKeyValue(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            if (arguments.Length > 3)
            {
                string path = GetFullPath((arguments[0] as NSJSString)?.Value);
                string section = (arguments[1] as NSJSString)?.Value;
                string key = (arguments[2] as NSJSString)?.Value;
                string value = (arguments[3] as NSJSString)?.Value;
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!string.IsNullOrEmpty(section) &&
                    !string.IsNullOrEmpty(key) &&
                    !string.IsNullOrEmpty(path))
                {
                    success = SetKeyValue(path, section, key, value);
                }
            }
            arguments.SetReturnValue(success);
        }
    }
}
