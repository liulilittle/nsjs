namespace nsjsdotnet.Core.IO
{
    using System;
    using System.IO;
    using System.Text;

    public static class FileAuxiliary
    {
        public static Encoding GetEncoding(byte[] s)
        {
            Encoding encoding = Encoding.Default;
            if (s == null || s.Length < 3)
            {
                return encoding;
            }
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; // 带BOM 
            if (s[0] == 0xEF && s[1] == 0xBB && s[2] == 0xBF)
            {
                encoding = Encoding.UTF8;
            }
            else if (s[0] == 0xFE && s[1] == 0xFF && s[2] == 0x00)
            {
                encoding = Encoding.BigEndianUnicode;
            }
            else if (s[0] == 0xFF && s[1] == 0xFE && s[2] == 0x41)
            {
                encoding = Encoding.Unicode;
            }
            return encoding;
        }

        public static bool TryReadAllText(string path, out string value)
        {
            return TryReadAllText(path, null, out value);
        }

        public static bool TryReadAllText(string path, Encoding encoding, out string value)
        {
            value = null;
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                byte[] buffer = File.ReadAllBytes(path);
                if (encoding == null)
                {
                    encoding = GetEncoding(buffer);
                }
                value = encoding.GetString(buffer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int GetFileLength(string path)
        {
            return unchecked((int)GetFileLength64(path));
        }

        public static long GetFileLength64(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return 0;
                }
                FileInfo info = new FileInfo(path);
                return info.Length;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
