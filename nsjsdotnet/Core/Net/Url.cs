namespace nsjsdotnet.Core.Net
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using Encoding_ = System.Text.Encoding;

    public static class Url
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private unsafe static extern sbyte* SetHandleCount(byte[] value);

        private unsafe static int strlen(sbyte* p)
        {
            int i = 0;
            if (p != null)
            {
                while (*p++ != 0)
                {
                    i++;
                }
            }
            return i;
        }

        public unsafe static string Encoding(string value, Encoding_ encoding)
        {
            if (encoding == null)
            {
                encoding = Encoding_.UTF8;
            }
            string eax;
            string str = string.Empty;
            byte[] buffer = encoding.GetBytes(value);
            fixed (byte* pinned = buffer)
            {
                sbyte* ptr = (sbyte*)pinned;
                for (int i = 0; i < buffer.Length; i++, ptr++)
                {
                    int asc = *ptr < 0 ? 256 + *ptr : *ptr;
                    if (asc < 42 || asc == 43 || asc > 57 && asc < 64 || asc > 90 && asc < 95 || asc == 96 || asc > 122)
                    {
                        eax = Convert.ToString(asc, 16);
                        str += eax.Length < 2 ? "%0" + eax : "%" + eax;
                    }
                    else
                    {
                        str += (char)asc;
                    }
                }
                return str;
            }
        }

        public static string Decoding(string URIstring, Encoding_ encoding)
        {
            if (encoding == null)
            {
                encoding = Encoding_.UTF8;
            }
            ArrayList str = new ArrayList();
            for (int i = 0; i < URIstring.Length; i++)
            {
                if (URIstring[i] == '%')
                {
                    string eax = URIstring.Substring(++i, 2);
                    str.Add(Convert.ToByte(eax, 16)); i++;
                }
                else
                {
                    str.Add(Convert.ToByte(URIstring[i]));
                }
            }
            return encoding.GetString((byte[])str.ToArray(typeof(byte)));
        }
    }
}
