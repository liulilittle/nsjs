namespace nsjsdotnet.Runtime.Systematic.IO
{
    using global::System;
    using global::System.IO;
    using global::System.Text;
    using FILE = global::System.IO.File;
    using FStream = global::System.IO.FileStream;
    using NSJSEncoding = nsjsdotnet.Runtime.Systematic.Text.Encoding;

    static unsafe class Text
    {
        public static Encoding GetEncoding(string path)
        {
            if (!FILE.Exists(path))
            {
                throw new FileNotFoundException("path");
            }
            FileInfo info = new FileInfo(path);
            return GetEncoding(info);
        }

        public static Encoding GetEncoding(FileInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            using (FStream fs = info.Open(FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[3];
                int len = fs.Read(buffer, 0, 3);
                if (len < 2)
                {
                    return Encoding.Default;
                }
                fixed (byte* pinned = buffer)
                {
                    return GetEncoding(pinned);
                }
            }
        }

        public static Encoding GetEncoding(byte[] buffer)
        {
            return GetEncoding(buffer, 0);
        }

        public static Encoding GetEncoding(byte[] buffer, int ofs)
        {
            if (buffer == null || ofs < 0 || (buffer.Length - ofs) < 3)
            {
                return NSJSEncoding.DefaultEncoding;
            }
            fixed (byte* src = &buffer[ofs])
            {
                byte* stream = src;
                if (stream != null)
                {
                    stream += ofs;
                }
                return GetEncoding(stream);
            }
        }

        public static Encoding GetEncoding(byte* buffer)
        {
            if (buffer == null)
            {
                return NSJSEncoding.DefaultEncoding;
            }
            if (buffer[0] >= 0xEF)
            {
                if (buffer[0] == 0xEF && buffer[1] == 0xBB) // {0xEF, 0xBB, 0xBF}
                {
                    return Encoding.UTF8;
                }
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF) // {0xFE, 0xFF}
                {
                    return Encoding.BigEndianUnicode;
                }
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE) // {0xFF, 0xFE}
                {
                    return Encoding.Unicode;
                }
                else
                {
                    return Encoding.Default;
                }
            }
            else
            {
                return Encoding.Default;
            }
        }

        public static void GetEncoding(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            Encoding encoding = NSJSEncoding.DefaultEncoding;
            if (arguments.Length > 0)
            {
                string path = (arguments[0] as NSJSString)?.Value;
                if (path != null)
                {
                    encoding = GetEncoding(path);
                }
                else
                {
                    byte[] buffer = (arguments[0] as NSJSUInt8Array)?.Buffer;
                    int offset = 0;
                    if (buffer != null && arguments.Length > 1)
                    {
                        NSJSInt32 i = arguments[1] as NSJSInt32;
                        offset = (i == null ? 0x00 : i.Value);
                    }
                    encoding = GetEncoding(buffer, offset);
                }
            }
            arguments.SetReturnValue(NSJSEncoding.New(arguments.VirtualMachine, encoding));
        }
    }
}
