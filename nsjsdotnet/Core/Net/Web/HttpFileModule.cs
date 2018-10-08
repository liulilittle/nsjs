namespace nsjsdotnet.Core.Net.Web
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.IO;
    using System;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    sealed class HttpFileModule
    {
        private HttpRequest request = null;
        private HttpResponse response = null;
        private HttpApplication application = null;
        // 1:GET, 2:POST, 3:HEAD, 4:DELETE
        private int methodid = -1;

        public HttpFileModule(HttpApplication application, HttpContext context)
        {
            this.application = application;
            this.request = context.Request;
            this.response = context.Response;
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
                response.ContentType = HttpContentTypeTable.Get(ext = ext.ToLower());
                if (string.IsNullOrEmpty(response.ContentType))
                {
                    response.ContentType = "application/octet-stream";
                }
                response.ContentEncoding = FileAuxiliary.GetEncoding(path);
                this.ProcessRequest(methodid, path);
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

        private void ProcessRequest(int methodid, string path)
        {
            try
            {
                if (methodid == 1)
                {
                    int? from;
                    int? to;
                    this.TryGetRange(out from, out to);
                    this.WriteFileToContentStream(path, from, to);
                }
                else if (methodid == 3)
                {
                    this.SetContentLengthByFile(path);
                }
                else
                {
                    response.StatusCode = 405;
                }
            }
            catch (Exception) { /*--A--*/ }
        }

        private void SetContentLengthByFile(string path)
        {
            try
            {
                response.StatusCode = 500;
                if (File.Exists(path))
                {
                    response.StatusCode = 200;
                    response.ContentLength = FileAuxiliary.GetFileLength(path);
                }
            }
            catch (Exception) { /*--A--*/ }
        }

        private unsafe void OpenFileViewAccessor(string path, Action<IntPtr, long> callback)
        {
            if (callback != null)
            {
                if (File.Exists(path))
                {
                    using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(path))
                    {
                        using (MemoryMappedViewAccessor mmva = mmf.CreateViewAccessor())
                        {
                            SafeHandle handle = mmva.SafeMemoryMappedViewHandle;
                            callback(handle.DangerousGetHandle(), FileAuxiliary.GetFileLength(path));
                        }
                    }
                }
            }
        }

        private unsafe void WriteFileToContentStream(string path, int? from, int? to)
        {
            try
            {
                response.Headers.Add(HttpResponseHeader.AcceptRanges, "bytes");
                this.OpenFileViewAccessor(path, (accessor, count) =>
                {
                    try
                    {
                        byte* stream = (byte*)accessor;
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
                        response.ContentLength = count;
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
        }
    }
}
