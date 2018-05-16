namespace nsjsdotnet.Core.IO
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class FileAuxiliary
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int _lclose(IntPtr hFile);

        private static readonly IntPtr INVALID_HANDLE_VALUE = unchecked((IntPtr)(-1));
        private const int OF_READWRITE = 2;
        private const int OF_READ = 0;
        private const int OF_WRITE = 1;
        private const int OF_SHARE_COMPAT = 0;

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
            IntPtr hFile = _lopen(path, OF_SHARE_COMPAT | OF_READ);
            if (hFile != INVALID_HANDLE_VALUE)
            {
                int count = GetFileSize(hFile, IntPtr.Zero);
                _lclose(hFile);
                return count;
            }
            return 0;
        }
    }
}
