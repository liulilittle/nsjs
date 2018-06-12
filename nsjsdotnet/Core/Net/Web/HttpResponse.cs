namespace nsjsdotnet.Core.Net.Web
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

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

        public bool End()
        {
            try
            {
                bool doEvt = false;
                lock (this)
                {
                    if (!this.closed)
                    {
                        doEvt = true;
                        this.closed = true;
                    }
                }
                if (doEvt)
                {
                    HttpContext context = this.CurrentContext;
                    if (context == null)
                    {
                        throw new InvalidOperationException("context");
                    }
                    HttpApplication application = context.Application;
                    application.OnEndProcessRequest(context);
                }
                this.response.Close(); // this.response.Abort();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Close()
        {
            return this.End();
        }

        public bool Abort()
        {
            return this.End();
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
                StreamWriter sw = new StreamWriter(response.OutputStream, response.ContentEncoding);
                sw.Write(s);
                sw.Flush();
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
                StreamWriter sw = new StreamWriter(response.OutputStream, response.ContentEncoding);
                sw.Write(c);
                sw.Flush();
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
                StreamWriter sw = new StreamWriter(response.OutputStream, response.ContentEncoding);
                sw.Write(buffer, index, count);
                sw.Flush();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool BinaryWrite(byte[] buffer, int index, int count)
        {
            try
            {
                if (buffer == null)
                {
                    return false;
                }
                Stream s = response.OutputStream;
                s.Write(buffer, index, count);
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
