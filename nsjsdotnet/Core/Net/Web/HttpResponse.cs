namespace nsjsdotnet.Core.Net.Web
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    public class HttpResponse
    {
        private HttpListenerResponse response = null;
        private bool closed = false;

        public HttpContext CurrentContext
        {
            get;
            internal set;
        }

        internal HttpResponse(HttpListenerResponse response)
        {
            this.response = response;
        }

        public string ContentType
        {
            get
            {
                return response.ContentType;
            }
            set
            {
                response.ContentType = value;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return response.Headers;
            }
            set
            {
                response.Headers = value;
            }
        }

        public bool KeepAlive
        {
            get
            {
                return response.KeepAlive;
            }
            set
            {
                response.KeepAlive = value;
            }
        }

        public bool SendChunked
        {
            get
            {
                return response.SendChunked;
            }
            set
            {
                response.SendChunked = value;
            }
        }

        public CookieCollection Cookies
        {
            get
            {
                return response.Cookies;
            }
            set
            {
                response.Cookies = value;
            }
        }

        public string StatusDescription
        {
            get
            {
                return response.StatusDescription;
            }
            set
            {
                response.StatusDescription = value;
            }
        }

        public int StatusCode
        {
            get
            {
                return response.StatusCode;
            }
            set
            {
                response.StatusCode = value;
                if (value == 200)
                {
                    response.StatusDescription = "OK";
                }
            }
        }

        public string RedirectLocation
        {
            get
            {
                return response.RedirectLocation;
            }
            set
            {
                response.RedirectLocation = value;
            }
        }

        public Stream OutputStream
        {
            get
            {
                return response.OutputStream;
            }
        }

        public long ContentLength
        {
            get
            {
                return response.ContentLength64;
            }
            set
            {
                response.ContentLength64 = value;
            }
        }

        public Version ProtocolVersion
        {
            get
            {
                return response.ProtocolVersion;
            }
            set
            {
                response.ProtocolVersion = value;
            }
        }

        public Encoding ContentEncoding
        {
            get
            {
                return response.ContentEncoding;
            }
            set
            {
                response.ContentEncoding = value;
            }
        }

        public bool AddHeader(string name, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }
                response.AddHeader(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AppendCookie(Cookie cookie)
        {
            try
            {
                if (cookie == null)
                {
                    return false;
                }
                response.AppendCookie(cookie);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AppendHeader(string name, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }
                response.AppendHeader(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void End()
        {
            bool doEvtAndClosed = false;
            lock (this)
            {
                if (!this.closed)
                {
                    doEvtAndClosed = true;
                    this.closed = true;
                }
            }
            if (doEvtAndClosed)
            {
                HttpContext context = this.CurrentContext;
                if (context == null)
                {
                    throw new InvalidOperationException("context");
                }
                HttpApplication application = context.Application;
                application.OnEndProcessRequest(context);
            }
            if (doEvtAndClosed)
            {
                Stream output = response.OutputStream;
                lock (output)
                {
                    try { output.Flush(); } catch (Exception) { }
                }
                int retry_count = 0;
                while (retry_count < 10)
                {
                    try
                    {
                        response.Close();
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        retry_count++;
                        Thread.Sleep(100);
                    }
                    catch (HttpListenerException) // 企图在不存在的网络连接上进行操作。
                    {
                        break;
                    }
                }
            }
        }

        public void Close()
        {
            this.End();
        }

        public bool Abort()
        {
            try
            {
                response.Abort();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Redirect(string url)
        {
            try
            {
                response.Redirect(url);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SetCookie(Cookie cookie)
        {
            try
            {
                if (cookie == null)
                {
                    return false;
                }
                response.SetCookie(cookie);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Write(string s)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    return false;
                }
                Stream ns = response.OutputStream;
                if (ns == null)
                {
                    return false;
                }
                Encoding eocoding = response.ContentEncoding ?? Encoding.UTF8;
                byte[] buffer = eocoding.GetBytes(s);
                ns.Write(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Write(char c)
        {
            try
            {
                Stream ns = response.OutputStream;
                if (ns == null)
                {
                    return false;
                }
                Encoding eocoding = response.ContentEncoding ?? Encoding.UTF8;
                byte[] buffer = eocoding.GetBytes(new[] { c });
                ns.Write(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Write(char[] buffer, int index, int count)
        {
            try
            {
                if (buffer == null)
                {
                    return false;
                }
                Stream ns = response.OutputStream;
                if (ns == null)
                {
                    return false;
                }
                Encoding eocoding = response.ContentEncoding ?? Encoding.UTF8;
                byte[] data = eocoding.GetBytes(buffer, index, count);
                ns.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool BinaryWrite(byte[] buffer)
        {
            return this.BinaryWrite(buffer, 0, buffer.Length);
        }

        public bool WriteFile(IntPtr fileHandle, long offset, long size)
        {
            try
            {
#pragma warning disable CS0618 // 类型或成员已过时
                using (FileStream fs = new FileStream(fileHandle, FileAccess.Read))
#pragma warning restore CS0618 // 类型或成员已过时
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    byte[] buf = new byte[512];
                    int ofs = 0;
                    while (ofs < size)
                    {
                        int len = fs.Read(buf, 0, buf.Length);
                        if (len > 0)
                        {
                            this.BinaryWrite(buf, 0, len);
                        }
                        ofs += len;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool WriteFile(string filename, long offset, long size)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    return false;
                }
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    SafeFileHandle h = fs.SafeFileHandle;
                    return this.WriteFile(h.DangerousGetHandle(), offset, size);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool WriteFile(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    return false;
                }
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    SafeFileHandle h = fs.SafeFileHandle;
                    return WriteFile(h.DangerousGetHandle(), 0, fs.Length);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
