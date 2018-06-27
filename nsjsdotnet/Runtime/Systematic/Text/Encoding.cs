namespace nsjsdotnet.Runtime.Systematic.Text
{
    using global::System;
    using global::System.Collections.Generic;
    using nsjsdotnet.Core;
    using ENCODING = global::System.Text.Encoding;

    sealed class Encoding
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static Encoding()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("GetEncoding", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetEncoding));
            owner.Set("GetEncodings", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetEncodings));
        }

        private static NSJSFunctionCallback g_GetBytesProc = null;
        private static NSJSFunctionCallback g_GetStringProc = null;
        private static object g_Locker = new object();
        private static bool g_UninitializedEncoding = false;
        private static IDictionary<int, IDictionary<IntPtr, NSJSObject>> g_EncodingInstanceTable = new Dictionary<int, IDictionary<IntPtr, NSJSObject>>();

        public static NSJSObject New(NSJSVirtualMachine machine, ENCODING encoding)
        {
            lock (g_Locker)
            {
                if (machine == null || encoding == null)
                {
                    return null;
                }
                IDictionary<IntPtr, NSJSObject> dVirtualTables;
                if (!g_EncodingInstanceTable.TryGetValue(encoding.CodePage, out dVirtualTables))
                {
                    dVirtualTables = new Dictionary<IntPtr, NSJSObject>();
                    g_EncodingInstanceTable.Add(encoding.CodePage, dVirtualTables);
                }
                NSJSObject o;
                if (dVirtualTables.TryGetValue(machine.Isolate, out o))
                {
                    return o;
                }
                if (!g_UninitializedEncoding)
                {
                    g_UninitializedEncoding = true;
                    g_GetBytesProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetBytes);
                    g_GetStringProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetString);
                }
                o = NSJSObject.New(machine);
                o.CrossThreading = true;
                o.Set("GetBytes", g_GetBytesProc);
                o.Set("GetString", g_GetStringProc);
                dVirtualTables.Add(machine.Isolate, o);
                NSJSKeyValueCollection.Set(o, encoding);
                return o;
            }
        }

        public static readonly ENCODING DefaultEncoding = ENCODING.UTF8;

        public static ENCODING GetEncoding(NSJSObject value)
        {
            if (value == null)
            {
                return DefaultEncoding;
            }
            return NSJSKeyValueCollection.Get<ENCODING>(value);
        }

        private static void GetBytes(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            ENCODING encoding = GetEncoding(arguments.This);
            if (encoding == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                byte[] buffer = null;
                if (arguments.Length > 0)
                {
                    NSJSString s = arguments[0] as NSJSString;
                    if (s != null)
                    {
                        buffer = encoding.GetBytes(s.Value);
                    }
                }
                if (buffer == null)
                {
                    buffer = BufferExtension.EmptryBuffer;
                }
                arguments.SetReturnValue(buffer);
            }
        }

        private static void GetString(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            ENCODING encoding = NSJSKeyValueCollection.Get<ENCODING>(arguments.This);
            if (encoding == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                string s = null;
                if (arguments.Length > 0)
                {
                    NSJSUInt8Array chars = arguments[0] as NSJSUInt8Array;
                    if (chars != null)
                    {
                        byte[] buffer = chars.Buffer;
                        if (buffer != null)
                        {
                            NSJSInt32 index = null;
                            NSJSInt32 len = null;
                            switch (arguments.Length)
                            {
                                case 2:
                                    len = arguments[1] as NSJSInt32;
                                    break;
                                case 3:
                                    index = arguments[1] as NSJSInt32;
                                    len = arguments[2] as NSJSInt32;
                                    break;
                            }
                            int ofs = index != null ? index.Value : 0;
                            int count = len != null ? len.Value : buffer.Length;
                            if (count < 0)
                            {
                                count = 0;
                            }
                            if (ofs < 0)
                            {
                                ofs = 0;
                            }
                            s = encoding.GetString(buffer, ofs, count);
                        }
                    }
                }
                if (s != null)
                {
                    arguments.SetReturnValue(s);
                }
                else
                {
                    arguments.SetReturnValue(NSJSValue.Undefined(arguments.VirtualMachine));
                }
            }
        }

        private static void GetEncoding(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue result = null;
            if (arguments.Length > 0)
            {
                NSJSInt32 codepage = arguments[0] as NSJSInt32;
                try
                {
                    if (codepage != null)
                    {
                        result = New(arguments.VirtualMachine, ENCODING.GetEncoding(codepage.Value));
                    }
                    NSJSString name = arguments[0] as NSJSString;
                    if (name != null)
                    {
                        result = New(arguments.VirtualMachine, ENCODING.GetEncoding(name.Value));
                    }
                }
                catch (Exception) { }
            }
            arguments.SetReturnValue(NSJSValue.UndefinedMerge(arguments.VirtualMachine, result));
        }

        private static void GetEncodings(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            var encodings = ENCODING.GetEncodings();
            arguments.SetReturnValue(ArrayAuxiliary.ToArray(arguments.VirtualMachine, encodings));
        }
    }
}
