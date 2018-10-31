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
    using System.Threading;

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

        private bool TryGetRange(out long? from, out long? to)
        {
            from = null;
            to = null;
            try
            {
                string range = request.Headers["Range"];
                if (!string.IsNullOrEmpty(range))
                {
                    range = range.ToLower();
                    Match match = Regex.Match(range, @"bytes=(\d+)-*(\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (match.Success)
                    {
                        long start;
                        if (long.TryParse(match.Groups[1].Value, out start))
                        {
                            from = start;
                        }
                        long end;
                        if (long.TryParse(match.Groups[2].Value, out end))
                        {
                            to = end;
                        }
                    }
                }
                return from != null || to != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Tuple<long, long, int> CalcAccessFileRange(long totalcount)
        {
            long? from;
            long? to;
            TryGetRange(out from, out to);
            return CalcAccessFileRange(from, to, totalcount);
        }

        private Tuple<long, long, int> CalcAccessFileRange(long? from, long? to, long totalcount)
        {
            long beg_pos = 0;
            long end_pos = 0;
            int cmp_oxx = 2;
            if (from.HasValue && to.HasValue) // range
            {
                beg_pos = (from.Value < 0) ? 0 : from.Value; // 206
                end_pos = (to.Value < 0) ? 0 : to.Value;
                cmp_oxx = 0;
            }
            else if (from.HasValue) // 0-
            {
                beg_pos = (from.Value < 0) ? 0 : from.Value;
                end_pos = totalcount;
                cmp_oxx = 1;
            }
            else if (to.HasValue) // -0
            {
                end_pos = (to.Value < 0) ? 0 : to.Value;
                cmp_oxx = -1;
            }
            else
            {
                end_pos = totalcount;
            }
            beg_pos = (beg_pos > totalcount) ? totalcount : beg_pos;
            end_pos = (end_pos > totalcount) ? totalcount : end_pos;
            if (beg_pos > end_pos)
            {
                end_pos = beg_pos;
            }
            return new Tuple<long, long, int>(beg_pos, end_pos, cmp_oxx);
        }

        private HttpContext GetCurrentContext()
        {
            return response.CurrentContext;
        }

        private unsafe void WriteBufferToClient(byte* buffer, long count, Action<bool, Exception> callback, bool asynchronous)
        {
            Exception exception = null;
            if (buffer == null)
            {
                exception = new ArgumentNullException("buffer");
            }
            else if (count <= 0)
            {
                exception = new ArgumentOutOfRangeException("count");
            }
            if (exception != null)
            {
                if (callback != null)
                {
                    callback(asynchronous, exception);
                }
                return;
            }
            ThreadStart output_thread_start = () =>
            {
                Stream output = response.OutputStream;
                byte[] chunk = new byte[1400];
                long offset = 0;
                lock (output)
                {
                    while (offset < count && exception == null)
                    {
                        long rdsz = unchecked(count - offset);
                        if (rdsz > chunk.Length)
                        {
                            rdsz = chunk.Length;
                        }
                        int len = unchecked((int)rdsz);
                        BufferExtension.memcpy(&buffer[offset], chunk, 0, len);
                        try
                        {
                            output.Write(chunk, 0, len);
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                        offset = unchecked(offset + rdsz);
                    }
                }
                if (callback != null)
                {
                    callback(asynchronous, exception);
                }
            };
            if (!asynchronous)
            {
                output_thread_start();
            }
            else
            {
                Thread output_thread_inst = new Thread(output_thread_start);
                output_thread_inst.IsBackground = true;
                output_thread_inst.Priority = ThreadPriority.Lowest;
                output_thread_inst.Start();
            }
        }

        private bool IfRangeValueIsEquals(params string[] s)
        {
            if (s == null || s.Length<=0)
            {
                return false;
            }
            string x = request.Headers["If-Range"];
            if (x == null)
            {
                return false;
            }
            x = x.Trim();
            for (int i = 0; i < s.Length; i++)
            {
                string y = s[i];
                if (y == null)
                {
                    continue;
                }
                y = y.Trim();
                if (y == x)
                {
                    return true;
                }
            }
            return false;
        }

        private unsafe void ProcessRequest(int methodid, string path)
        {
            try
            {
                if (methodid == 1)
                {
                    response.AddHeader("Accept-Ranges", "bytes");
                    response.KeepAlive = true;
                    response.AddHeader("Connection", "Keep-Alive");

                    this.OpenFileViewAccessor(path, (accessor, total_count, close_mmf) =>
                    {
                        byte* stream = (byte*)accessor;

                        response.StatusCode = 200;

                        long write_count = total_count;
                        Tuple<long, long, int> range = CalcAccessFileRange(total_count);
                        write_count = (range.Item2 - range.Item1);
                        if (write_count > 0)
                        {
                            stream = &stream[range.Item1];
                        }
                        response.ContentLength = write_count;
                        if (range.Item1 == range.Item2 || 
                            range.Item3 == 2)
                        {
                            response.StatusCode = 200;
                        }
                        else
                        {
                            response.StatusCode = 206;
                            response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}",
                                range.Item1,
                                (range.Item2 - 1),
                                total_count)
                            );
                        }
                        WriteBufferToClient(stream, write_count, (asynchronous, e) =>
                        {
                            close_mmf();
                            response.End(asynchronous);
                        }, false);
                    });
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

        private unsafe void OpenFileViewAccessor(string path, Action<IntPtr, long, Action> callback)
        {
            if (callback == null || !File.Exists(path))
            {
                response.StatusCode = 404;
                return;
            }
            MemoryMappedFile mmf = null;
            MemoryMappedViewAccessor mmva = null;
            Action closeMmf = () =>
            {
                if (mmva != null)
                {
                    mmva.Dispose();
                    mmva = null;
                }
                if (mmf != null)
                {
                    mmf.Dispose();
                    mmf = null;
                }
            };
            bool immediate_shutdown = false;
            try
            {
                mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open);
                long totalcount = FileAuxiliary.GetFileLength64(path);
                mmva = mmf.CreateViewAccessor(0, totalcount, MemoryMappedFileAccess.ReadWrite);
                SafeHandle handle = mmva.SafeMemoryMappedViewHandle;
                if (handle.IsInvalid)
                {
                    immediate_shutdown = true;
                    response.StatusCode = 503;
                }
                else
                {
                    callback(handle.DangerousGetHandle(), totalcount, closeMmf);
                }
            }
            catch (Exception)
            {
                immediate_shutdown = true;
                response.StatusCode = 500;
            }
            if (immediate_shutdown)
            {
                closeMmf();
            }
        }
    }
}
