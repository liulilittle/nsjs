namespace nsjsdotnet.Core.Net.Web
{
    using nsjsdotnet.Core;
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    sealed class HttpFileModule
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

        private static class NativeMethods
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
            public const int PAGE_READONLY = 2;
            public const int PAGE_READWRITE = 4;
            public const int FILE_MAP_READ = 4;
            public const int FILE_MAP_WRITE = 2;
            public const int OF_READWRITE = 2;
        }

        private int GetMethodId()
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
            int methodid = this.GetMethodId();
            if (methodid != 1 && methodid != 3)
            {
                return false;
            }
            string path = (application.Root + request.Url.LocalPath);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                return false;
            }
            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }
            try
            {
                response.ContentType = HttpContentTypes.Get(ext = ext.ToLower());
                if (string.IsNullOrEmpty(response.ContentType))
                {
                    response.ContentType = "application/octet-stream";
                }
                this.ProcessReponse(methodid, path);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool TryGetRange(out int? from, out int? to)
        {
            try
            {
                string range = request.Headers["Range"];
                from = null;
                to = null;
                if (!string.IsNullOrEmpty(range))
                {
                    range = range.ToLower();
                    Match match = Regex.Match(range, @"bytes=(\d+)-*(\d+)*");
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
            catch (Exception)
            {
                from = null;
                to = null;
                return false;
            }
        }

        private void ProcessReponse(int methodid, string path)
        {
            try
            {
                if (methodid == 1)
                {
                    int? from;
                    int? to;
                    this.TryGetRange(out from, out to);
                    this.ProcessStream(path, from, to);
                }
                else if (methodid == 3)
                {
                    this.ProcessStreamSize(path);
                }
                else
                {
                    response.StatusCode = 405;
                }
            }
            catch (Exception) { /*--A--*/ }
        }

        private void ProcessStreamSize(string path)
        {
            IntPtr hf = NativeMethods._lopen(path, NativeMethods.OF_SHARE_COMPAT | NativeMethods.OF_READWRITE);
            do
            {
                try
                {
                    response.StatusCode = 500;
                    if (hf == NativeMethods.INVALID_HANDLE_VALUE)
                    {
                        break;
                    }
                    else
                    {
                        long size = 0;
                        if (!NativeMethods.GetFileSizeEx(hf, ref size))
                        {
                            break;
                        }
                        response.StatusCode = 200;
                        response.ContentLength64 = size;
                    }
                }
                catch (Exception) { /*--A--*/ }
            } while (false);
            if (hf != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods._lclose(hf);
            }
        }

        private unsafe void ProcessStream(string path, Action<IntPtr, long> callback)
        {
            IntPtr hf = NativeMethods.INVALID_HANDLE_VALUE;
            IntPtr hm = NativeMethods.NULL;
            IntPtr pl = NativeMethods.NULL;
            do
            {
                hf = NativeMethods._lopen(path, NativeMethods.OF_SHARE_COMPAT | NativeMethods.OF_READWRITE);
                if (hf == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    break;
                }
                long size = 0;
                if (!NativeMethods.GetFileSizeEx(hf, ref size) || size <= 0)
                {
                    break;
                }
                hm = NativeMethods.CreateFileMapping(hf, NativeMethods.NULL, NativeMethods.PAGE_READWRITE, unchecked((uint)(size >> 32)), unchecked((uint)size), null);
                if (hm == NativeMethods.NULL)
                {
                    break;
                }
                pl = NativeMethods.MapViewOfFile(hm, NativeMethods.FILE_MAP_READ | NativeMethods.FILE_MAP_WRITE, 0, 0, NativeMethods.NULL);
                if (pl == NativeMethods.NULL)
                {
                    break;
                }
                if (callback != null)
                {
                    callback(pl, size);
                }
            } while (false);
            if (pl != NativeMethods.NULL)
            {
                NativeMethods.UnmapViewOfFile(pl);
            }
            if (hm != NativeMethods.NULL)
            {
                NativeMethods._lclose(hm);
            }
            if (hf != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.CloseHandle(hf);
            }
        }

        private unsafe void ProcessStream(string path, int? from, int? to)
        {
            try
            {
                response.Headers.Add(HttpResponseHeader.AcceptRanges, "bytes");
                this.ProcessStream(path, (pfile, count) =>
                {
                    try
                    {
                        byte* stream = (byte*)pfile;
                        if (from.HasValue && to.HasValue) // range
                        {
                            int ofs = from.Value < 0 ? 0 : from.Value; // 206
                            count = to.Value < 0 ? 0 : to.Value;
                            stream = count > ofs ? &stream[ofs] : null;
                        }
                        else if (from.HasValue || to.HasValue) // 206
                        {
                            int ofs = 0;
                            if (from.HasValue)
                            {
                                ofs = from.Value;
                            }
                            else if (to.HasValue)
                            {
                                ofs = to.Value;
                            }
                            if (ofs < 0)
                            {
                                ofs = 0;
                            }
                            count = count - ofs;
                            if (count < 0)
                            {
                                count = 0;
                            }
                            stream = count > ofs ? &stream[ofs] : null;
                        }
                        response.StatusCode = 200;
                        response.ContentLength64 = count;
                        if (count > 0)
                        {
                            Stream output = response.OutputStream;
                            byte[] chunk = new byte[570];
                            int ofs = 0;
                            while (ofs < count)
                            {
                                int len = unchecked((int)(count - ofs));
                                if (len > chunk.Length)
                                {
                                    len = chunk.Length;
                                }
                                BufferExtension.memcpy(&stream[ofs], chunk, 0, len);
                                output.Write(chunk, 0, len);
                                ofs += len;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        response.StatusCode = 500;
                    }
                });
            }
            catch (Exception) { /*--A--*/ }
            response.Close();
        }
    }
}
