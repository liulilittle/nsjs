namespace nsjsdotnet.Core.Net.Web
{
    using nsjsdotnet.Core;
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    class HttpFileModule
    {
        private HttpListenerContext context = null;
        private HttpListenerRequest request = null;
        private HttpListenerResponse response = null;
        private HttpApplication application = null;
        // 1:GET, 2:POST, 3:HEAD, 4:DELETE
        private int methodid = -1;

        public HttpFileModule(HttpApplication application, HttpListenerContext context)
        {
            this.application = application;
            this.context = context;
            this.request = context.Request;
            this.response = context.Response;
        }

        static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr _lcreat(string lpPathName, int iAttribute);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, int flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int UnmapViewOfFile(IntPtr hMapFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int _lclose(IntPtr hFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetFileSizeEx(IntPtr hFile, ref long lpFileSizeHigh);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int CloseHandle(IntPtr hObject);

            public static readonly IntPtr INVALID_HANDLE_VALUE = unchecked((IntPtr)(-1));
            public static readonly IntPtr NULL = IntPtr.Zero;
            public const int OF_SHARE_COMPAT = 0;
            // public const int PAGE_READONLY = 2;
            public const int PAGE_READWRITE = 4;
            public const int FILE_MAP_READ = 4;
            public const int FILE_MAP_WRITE = 2;
            public const int OF_READWRITE = 2;

            public static bool FileExits(string path)
            {
                IntPtr hFile = NativeMethods._lopen(path, NativeMethods.OF_SHARE_COMPAT | NativeMethods.OF_READWRITE);
                CloseHandle(hFile);
                return hFile == NativeMethods.INVALID_HANDLE_VALUE;
            }
        }

        private int GetMethod()
        {
            if (methodid == -1)
            {
                string method = request.HttpMethod;
                if (string.IsNullOrEmpty(method))
                {
                    methodid = 0;
                }
                else
                {
                    method = method.ToUpper();
                    if (method.Contains("GET"))
                    {
                        methodid = 1;
                    }
                    else if (method.Contains("POST"))
                    {
                        methodid = 2;
                    }
                    else if (method.Contains("HEAD"))
                    {
                        methodid = 3;
                    }
                    else if (method.Contains("DELETE"))
                    {
                        methodid = 4;
                    }
                    else
                    {
                        methodid = 0;
                    }
                }
            }
            return methodid;
        }

        public bool Handle()
        {
            int method = GetMethod();
            if (method != 1 && method != 3)
            {
                return false;
            }
            string path = (application.Root + request.Url.LocalPath);
            IntPtr hFile = NativeMethods._lopen(path, NativeMethods.OF_SHARE_COMPAT | NativeMethods.OF_READWRITE);
            if (hFile == NativeMethods.INVALID_HANDLE_VALUE)
            {
                return false;
            }
            NativeMethods._lclose(hFile);
            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }
            response.ContentType = HttpContentTypes.Get(ext = ext.ToLower());
            if (string.IsNullOrEmpty(response.ContentType))
            {
                response.ContentType = "application/octet-stream";
            }
            ProcessReponse(method, path);
            return true;
        }

        private bool TryGetRange(out int? from, out int? to)
        {
            string range = request.Headers["Range"];
            from = null;
            to = null;
            if (!string.IsNullOrEmpty(range))
            {
                range = range.ToLower();
                Match match = Regex.Match(range, "bytes[=|\\s](\\d+)-(\\d+)*");
                if (match.Success)
                {
                    int start;
                    if (int.TryParse(match.Groups[1].Value, out start))
                    {
                        from = start;
                    }
                    int end;
                    if (int.TryParse(match.Groups[2].Value, out end))
                    {
                        to = end;
                    }
                }
            }
            return from != null;
        }

        private void ProcessReponse(int method, string path)
        {
            if (method == 1)
            {
                int? from;
                int? to;
                TryGetRange(out from, out to);
                ProcessStream(path, from, to);
            }
            else if (method == 3)
            {
                ProcessStreamSize(path);
            }
            else
            {
                response.StatusCode = 405;
            }
        }

        private void ProcessStreamSize(string path)
        {
            IntPtr hFile = NativeMethods._lopen(path, NativeMethods.OF_SHARE_COMPAT | NativeMethods.OF_READWRITE);
            do
            {
                response.StatusCode = 500;
                if (hFile == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    break;
                }
                else
                {
                    long size = 0;
                    if (!NativeMethods.GetFileSizeEx(hFile, ref size))
                    {
                        break;
                    }
                    response.StatusCode = 200;
                    response.ContentLength64 = size;
                }
            } while (false);
            if (hFile != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods._lclose(hFile);
            }
            response.Close();
        }

        private unsafe void ProcessStream(string path, Action<IntPtr, long> callback)
        {
            IntPtr hFile = NativeMethods.INVALID_HANDLE_VALUE;
            IntPtr hMap = NativeMethods.NULL;
            IntPtr pLow = NativeMethods.NULL;
            do
            {
                hFile = NativeMethods._lopen(path, NativeMethods.OF_SHARE_COMPAT | NativeMethods.OF_READWRITE);
                if (hFile == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    break;
                }
                long size = 0;
                if (!NativeMethods.GetFileSizeEx(hFile, ref size) || size <= 0)
                {
                    break;
                }
                hMap = NativeMethods.CreateFileMapping(hFile, NativeMethods.NULL, NativeMethods.PAGE_READWRITE, (uint)(size >> 32), (uint)size, null);
                if (hMap == NativeMethods.NULL)
                {
                    break;
                }
                pLow = NativeMethods.MapViewOfFile(hMap, NativeMethods.FILE_MAP_READ | NativeMethods.FILE_MAP_WRITE, 0, 0, NativeMethods.NULL);
                if (pLow == NativeMethods.NULL)
                {
                    break;
                }
                if (callback != null)
                {
                    callback(pLow, size);
                }
            } while (false);
            if (pLow != NativeMethods.NULL)
            {
                NativeMethods.UnmapViewOfFile(pLow);
            }
            if (hMap != NativeMethods.NULL)
            {
                NativeMethods._lclose(hMap);
            }
            if (hFile != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.CloseHandle(hFile);
            }
        }

        private unsafe void ProcessStream(string path, int? from, int? to)
        {
            response.Headers.Add(HttpResponseHeader.AcceptRanges, "bytes");
            ProcessStream(path, (pfile, size) =>
            {
                try
                {
                    response.StatusCode = 200;
                    byte* buffer = (byte*)pfile;
                    if (!from.HasValue) // range
                    {
                        response.ContentLength64 = size;
                    }
                    else if (!from.HasValue && to.HasValue)
                    {
                        buffer += (size - to.Value);
                        size = to.Value;
                        response.StatusCode = 206;
                    }
                    else
                    {
                        buffer += from.Value;
                        long len = (size - from.Value);
                        if (len < size)
                        {
                            size = len;
                            response.StatusCode = 206;
                        }
                        if (to.HasValue)
                        {
                            len = (to.Value - from.Value);
                            size = len;
                            response.StatusCode = 206;
                        }
                        response.ContentLength64 = size;
                    }
                    if (size > 0)
                    {
                        Stream s = response.OutputStream;
                        byte[] chunk = new byte[570];
                        long ofs = 0;
                        while (ofs < size)
                        {
                            long chunksize = (size - ofs);
                            if (chunksize > chunk.Length)
                            {
                                chunksize = chunk.Length;
                            }
                            BufferExtension.memcpy(&buffer[ofs], chunk, 0, (int)chunksize);
                            s.Write(chunk, 0, (int)chunksize);
                            ofs += chunksize;
                        }
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = 500;
                }
            });
            response.Close();
        }
    }
}
