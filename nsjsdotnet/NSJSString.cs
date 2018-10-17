namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Diagnostics;
    using System.IO;

    public unsafe class NSJSString : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_string_new(IntPtr isolate, void* data, int datalen);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static bool IsBadReadPtr(void* lp, uint ucb);

        private const uint CP_UTF8 = 65001;

        private const byte kFirstBitMask = 0x80; // 1000000
        private const byte kSecondBitMask = 0x40; // 0100000
        private const byte kThirdBitMask = 0x20; // 0010000
        private const byte kFourthBitMask = 0x10; // 0001000
        private const byte kFifthBitMask = 0x08; // 0000100

        public static int GetUtf8Alignment(byte character)
        {
            int alignment = 1;
            if ((character & kFirstBitMask) > 0) // This means the first byte has a value greater than 127, and so is beyond the ASCII range.
            {
                if ((character & kThirdBitMask) > 0) // This means that the first byte has a value greater than 224, and so it must be at least a three-octet code point.
                {
                    if ((character & kFourthBitMask) > 0) // This means that the first byte has a value greater than 240, and so it must be a four-octet code point.
                    {
                        alignment = 4;
                    }
                    else
                    {
                        alignment = 3;
                    }
                }
                else
                {
                    alignment = 2;
                }
            }
            return alignment;
        }

        public static int GetUtf8BufferCount(byte* s)
        {
            if (s == null)
            {
                return 0;
            }
            byte* i = s;
            while (!IsBadReadPtr(i, 1))
            {
                int alignment = GetUtf8Alignment(*i);
                int character = 0;
                if (alignment == 1)
                {
                    character = *i++;
                }
                if (alignment == 2)
                {
                    character = *(short*)i;
                    i += 2;
                }
                else if (alignment == 3)
                {
                    character = (*i++ | *i++ << 8 | *i++ << 16);
                }
                else if (alignment == 4)
                {
                    character = *(int*)i;
                    i += 4;
                }
                if (character == 0)
                {
                    int len = unchecked((int)(i - (s + 1)));
                    return len < 0 ? 0 : len;
                }
            }
            return 0;
        }

        public static bool IsUTF8Buffer(byte[] buffer)
        {
            if (buffer == null)
            {
                return false;
            }
            fixed (byte* pinned = buffer)
            {
                return IsUTF8Buffer(pinned, buffer.Length);
            }
        }

        public static bool IsUTF8Buffer(byte* buffer, int count)
        {
            if (buffer == null || count <= 0)
            {
                return false;
            }
            int counter = 1;
            byte key = 0;
            for (int i = 0; i < count; i++)
            {
                key = buffer[i];
                if (counter == 1)
                {
                    if (key >= 0x80)
                    {
                        while (((key <<= 1) & 0x80) != 0)
                        {
                            counter++;
                        }
                        if (counter == 1 || counter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if ((key & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    counter--;
                }
            }
            return !(counter > 1);
        }

        public static byte[] GetUTF8StringBuffer(string s)
        {
            int count = 0;
            return GetUTF8StringBuffer(s, out count); 
        }

        public static byte[] GetUTF8StringBuffer(string s, out int count)
        {
            count = 0;
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            byte[] buf = null;
            int k = 0;
            int i = 0;
            while (i < s.Length)
            {
                char ch = s[i++];
                if (ch < 0x80)
                {
                    k++;
                }
                else if (ch < 0x800)
                {
                    k += 2;
                }
                else if (ch < 0x10000)
                {
                    k += 3;
                }
            }
            buf = new byte[(count = k) + 1];
            fixed (byte* p = buf)
            {
                i = 0;
                k = 0;
                while (i < s.Length)
                {
                    char ch = s[i++];
                    if (ch < 0x80)
                    {
                        p[k++] = (byte)(ch & 0xff);
                    }
                    else if (ch < 0x800)
                    {
                        p[k++] = (byte)(((ch >> 6) & 0x1f) | 0xc0);
                        p[k++] = (byte)((ch & 0x3f) | 0x80);
                    }
                    else if (ch < 0x10000)
                    {
                        p[k++] = (byte)(((ch >> 12) & 0x0f) | 0xe0);
                        p[k++] = (byte)(((ch >> 6) & 0x3f) | 0x80);
                        p[k++] = (byte)((ch & 0x3f) | 0x80);
                    }
                }
            }
            return buf;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value = default(object);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool initialized = false;

        internal NSJSString(IntPtr handle, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kString, machine)
        {

        }

        public static NSJSString Cast(NSJSValue value)
        {
            return Cast(value, (handle, machine) => new NSJSString(value.Handle, value.VirtualMachine));
        }

        public override object GetValue()
        {
            lock (this)
            {
                if (!this.initialized)
                {
                    this.initialized = true;
                    this.value = this.LocalValueToString();
                }
                return this.value;
            }
        }

        public string Value
        {
            get
            {
                return this.GetValue() as string;
            }
        }

        public static string Utf8ToString(byte[] buffer)
        {
            return Utf8ToString(buffer, 0, buffer.Length);
        }

        public static string Utf8ToString(byte[] buffer, int len)
        {
            return Utf8ToString(buffer, 0, len);
        }

        public static string Utf8ToString(byte[] buffer, int ofs, int len)
        {
            if (ofs == buffer.Length)
            {
                return Utf8ToString((byte*)null, len);
            }
            fixed (byte* p = &buffer[ofs])
            {
                return Utf8ToString(p, len);
            }
        }

        public static string Utf8ToString(void* p)
        {
            return Utf8ToString((IntPtr)p);
        }

        public static string Utf8ToString(void* p, int len)
        {
            return Utf8ToString((IntPtr)p, len);
        }

        public static string Utf8ToString(IntPtr p)
        {
            if (p == NULL)
            {
                return null;
            }
            int len = GetUtf8BufferCount(unchecked((byte*)p));
            if (len < 0)
            {
                return null;
            }
            return Utf8ToString(p, len);
        }

        public static string Utf8ToString(IntPtr p, int len)
        {
            if (p == NULL || len < 0)
            {
                return null;
            }
            if (len == 0)
            {
                return string.Empty;
            }
            return new string((sbyte*)p, 0, len, Encoding.UTF8);
        }

        public static NSJSValue NewValue(NSJSVirtualMachine machine, string value)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            if (value == null)
            {
                return NSJSValue.Null(machine);
            }
            return New(machine, value);
        }

        public static NSJSString New(NSJSVirtualMachine machine, string value)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            IntPtr handle = NULL;
            if (value == null)
            {
                handle = nsjs_localvalue_string_new(isolate, null, 0);
            }
            else
            {
                int count;
                byte[] cch = NSJSString.GetUTF8StringBuffer(value, out count);
                fixed (byte* p = cch)
                {
                    handle = nsjs_localvalue_string_new(isolate, p, count);
                }
            }
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSString(handle, machine);
        }

        public override string ToString()
        {
            return this.Value;
        }

        public static explicit operator string(NSJSString value)
        {
            return value == null ? null : value.ToString();
        }

        public static implicit operator bool(NSJSString value)
        {
            if (value == null || value.IsNullOrUndfined)
            {
                return false;
            }
            string s = value.Value;
            return unchecked(s != null && s.Length > 0);
        }
    }
}
