namespace nsjsdotnet.Core.Net
{
    using nsjsdotnet.Core.Utilits;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Text;
    using System.Threading;

    public class HttpPostValue
    {
        public bool IsText
        {
            get;
            set;
        }

        public string Key
        {
            get;
            set;
        }

        public byte[] Value
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }
    }

    public class HttpClientOptions
    {
        public static readonly HttpClientOptions DefaultOptions = new HttpClientOptions();

        public CookieContainer CookieContainer { get; set; }

        public string Referer { get; set; }

        public int Timeout { get; set; }

        public IWebProxy Proxy { get; set; }

        public int[] Range { get; set; }

        public RequestCachePolicy CachePolicy { get; set; } // new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

        public bool AllowAutoRedirect { get; set; }

        public int MaximumAutomaticRedirections { get; set; }

        public DecompressionMethods AutomaticDecompression { get; set; }

        public NameValueCollection Headers { get; set; }

        public object Tag { get; set; }

        public HttpClientOptions()
        {
            this.Timeout = 30000;
            this.CookieContainer = HttpClient.DefaultCookieContainer;
            this.Proxy = WebRequest.DefaultWebProxy;
            this.CachePolicy = WebRequest.DefaultCachePolicy;
            this.AllowAutoRedirect = true;
            this.MaximumAutomaticRedirections = 64;
            this.AutomaticDecompression = DecompressionMethods.None;
            this.Headers = new WebHeaderCollection();
        }
    }

    public class HttpClientResponse
    {
        public bool AsynchronousMode { get; set; }

        public bool ManualWriteToStream { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public Stream ContentStream { get; set; }

        public long ContentLength { get; set; }

        public string ContentType { get; set; }

        public string ContentEncoding { get; set; }

        public string StatusDescription { get; set; }

        public string Server { get; set; }

        public NameValueCollection Headers { get; set; }

        public CookieCollection Cookies { get; set; }

        public Uri ResponseUri { get; set; }

        public string CharacterSet { get; set; }

        public DateTime LastModified { get; set; }

        public object Tag { get; set; }
    }

    public enum HttpClientError
    {
        Success = 0,
        UnableOpenURL = 1,
        UnableToUploadBLOB = 2,
        ReadStreamInterruption = 3,
        UnableToSendRequest = 4,
        UnableToGetResponseStream = 5,
    }

    public delegate bool HttpClientAsyncCallback(HttpClientError error, byte[] buffer, int count);

    public class HttpClient
    {
        public readonly static CookieContainer DefaultCookieContainer = new CookieContainer();

        private static HttpWebRequest Create(string url, HttpClientOptions options)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = options.CookieContainer;
            request.Referer = string.IsNullOrEmpty(options.Referer) ? url : options.Referer;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.KeepAlive = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36";
            request.Headers.Add("Accept-Encoding: gzip, deflate");
            request.Headers.Add("DNT: 1");
            request.Headers.Add("Upgrade-Insecure-Requests: 1");
            request.Headers.Add("Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.CachePolicy = options.CachePolicy;
            request.AllowAutoRedirect = options.AllowAutoRedirect;
            request.Proxy = options.Proxy;
            request.Referer = options.Referer;
            request.Timeout = options.Timeout;
            request.MaximumAutomaticRedirections = options.MaximumAutomaticRedirections;
            request.AutomaticDecompression = options.AutomaticDecompression;
            int[] range = options.Range;
            if (range != null && range.Length > 0)
            {
                if (range.Length > 1)
                {
                    request.AddRange(range[0], range[1]);
                }
                else
                {
                    request.AddRange(range[0]);
                }
            }
            NameValueCollection collection = options.Headers;
            if (collection != null)
            {
                WebHeaderCollection headers = request.Headers;
                foreach (string key in collection.AllKeys)
                {
                    headers.Add(collection.Get(key));
                }
            }
            return request;
        }

        private static bool Fill(HttpWebResponse response, HttpClientResponse value)
        {
            if (response == null || value == null)
            {
                return false;
            }
            value.ContentLength = response.ContentLength;
            value.LastModified = response.LastModified;
            value.Headers = response.Headers;
            value.Cookies = response.Cookies;
            value.ResponseUri = response.ResponseUri;
            value.CharacterSet = response.CharacterSet;
            value.Server = response.Server;
            value.StatusCode = response.StatusCode;
            value.ContentEncoding = response.ContentEncoding;
            value.ContentType = response.ContentType;
            value.StatusDescription = response.StatusDescription;
            return true;
        }

        public static bool TryDownload(string url, HttpClientOptions options, HttpClientResponse response)
        {
            if (options == null || response == null)
            {
                return false;
            }
            response.AsynchronousMode = false;
            return HttpClient.InternalTryDownload(url, options, response, null);
        }

        private static bool InternalTryDownload(string url, HttpClientOptions options, HttpClientResponse response, HttpClientAsyncCallback callback)
        {
            if (options == null || response == null)
            {
                return false;
            }
            HttpWebRequest request = null;
            bool success = false;
            try
            {
                request = HttpClient.Create(url, options);
                request.Method = "GET"; // application/octet-stream
                request.ContentType = "text/html;charset=UTF-8";
                ParameterizedThreadStart copyto = delegate (object state)
                {
                    HttpWebResponse wr = null;
                    Stream rs = null;
                    try
                    {
                        using (wr = (HttpWebResponse)request.GetResponse())
                        {
                            HttpClient.Fill(wr, response);
                            using (rs = wr.GetResponseStream())
                            {
                                Func<byte[], int, bool> callbackt = null;
                                if (callback != null)
                                {
                                    callbackt = (buffer, len) =>
                                    {
                                        HttpClientError error = HttpClientError.Success;
                                        if (len < 0)
                                        {
                                            error = HttpClientError.ReadStreamInterruption;
                                        }
                                        return callback(error, buffer, len);
                                    };
                                }
                                success = HttpClient.CopyTo(rs, response, callbackt);
                            }
                        }
                        request.Abort();
                    }
                    catch (Exception)
                    {
                        if (wr != null && rs == null)
                        {
                            callback?.Invoke(HttpClientError.UnableToSendRequest, null, -1);
                        }
                        else if (rs != null)
                        {
                            callback?.Invoke(HttpClientError.UnableToGetResponseStream, null, -1);
                        }
                        else if (wr == null)
                        {
                            callback?.Invoke(HttpClientError.UnableOpenURL, null, -1);
                        }
                        if (rs != null)
                        {
                            rs.Close();
                        }
                        if (wr != null)
                        {
                            wr.Close();
                        }
                        if (request != null)
                        {
                            request.Abort();
                        }
                    }
                };
                if (!response.AsynchronousMode)
                {
                    copyto(response);
                }
                else
                {
                    (new Thread(copyto) { IsBackground = true, Priority = ThreadPriority.Lowest }).Start(response);
                }
                return success;
            }
            catch (Exception)
            {
                if (request != null)
                {
                    request.Abort();
                }
                return false;
            }
        }

        public static bool TryDownloadAsync(string url, HttpClientOptions options, HttpClientResponse response, HttpClientAsyncCallback callback)
        {
            if (options == null || response == null)
            {
                return false;
            }
            response.AsynchronousMode = true;
            return HttpClient.InternalTryDownload(url, options, response, callback);
        }

        private static bool CopyTo(Stream s, HttpClientResponse destination, Func<byte[], int, bool> callback = null)
        {
            if (s == null || destination == null)
            {
                return false;
            }
            Stream contentstream = destination.ContentStream;
            if (contentstream == null)
            {
                return false;
            }
            byte[] buffer = new byte[570];
            int len = 0;
            bool success = true;
            try
            {
                bool done = true;
                while ((len = s.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (callback != null && !callback(buffer, len))
                    {
                        done = false;
                        break;
                    }
                    if (!destination.ManualWriteToStream)
                    {
                        contentstream.Write(buffer, 0, len);
                    }
                }
                if (done)
                {
                    callback(buffer, 0);
                }
            }
            catch (Exception)
            {
                success = false;
                len = -1;
                if (callback != null)
                {
                    callback(buffer, len);
                }
            }
            if (contentstream.Position > destination.ContentLength)
            {
                destination.ContentLength = contentstream.Position;
            }
            contentstream.Position = 0;
            contentstream.Flush();
            return success;
        }

        private static bool PreparationUpload(HttpWebRequest request, IEnumerable<HttpPostValue> blobs)
        {
            if (request == null)
            {
                return false;
            }
            string tokenid = Guid.NewGuid().ToString(); // "D"
            string boundary = "---------------" + (ulong)Hash64.GetHashCode(tokenid);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            byte[] buffer = null;
            MemoryStream ms = null;
            try
            {
                if (blobs != null)
                {
                    ms = new MemoryStream();
                    buffer = new byte[570];
                    foreach (HttpPostValue blob in blobs)
                    {
                        if (blob == null)
                        {
                            throw new ArgumentNullException("blob");
                        }
                        if (blob.IsText)
                        {
                            byte[] cch = Encoding.UTF8.GetBytes(string.Format(
                                                "\r\n--{0}" +
                                                "\r\nContent-Disposition: form-data; name=\"{1}\"" +
                                                "\r\n\r\n{2}\r\n", boundary, blob.Key, blob.Value));
                            ms.Write(cch, 0, cch.Length);
                        }
                        else
                        {
                            byte[] cch = Encoding.UTF8.GetBytes(string.Format(
                                       "\r\n--{0}\r\n" +
                                       "Content-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\n" +
                                       "Content-Type: application/octet-stream\r\n\r\n", boundary, blob.Key, blob.FileName));
                            ms.Write(cch, 0, cch.Length);

                            cch = blob.Value;
                            if (cch != null)
                            {
                                ms.Write(cch, 0, cch.Length);
                            }
                            else
                            {
                                using (FileStream fs = new FileStream(blob.FileName, FileMode.Open, FileAccess.Read))
                                {
                                    try
                                    {
                                        int len = 0;
                                        while ((len = fs.Read(buffer, 0, buffer.Length)) != 0)
                                        {
                                            ms.Write(buffer, 0, len);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                if (blobs != null)
                {
                    byte[] cch = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");
                    ms.Write(cch, 0, cch.Length);
                }
                request.ContentLength = ms.Length;
                if (blobs != null)
                {
                    using (Stream rs = request.GetRequestStream())
                    {
                        ms.Position = 0;
                        int len = 0;
                        while ((len = ms.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            rs.Write(buffer, 0, len);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                }
            }
        }

        private static bool InternalTryUpload(string url, IEnumerable<HttpPostValue> blobs, HttpClientOptions options, HttpClientResponse response, HttpClientAsyncCallback callback)
        {
            if (options == null || response == null)
            {
                return false;
            }
            HttpWebRequest request = null;
            bool success = false;
            try
            {
                request = HttpClient.Create(url, options);
                ParameterizedThreadStart copyto = delegate (object state)
                {
                    HttpWebResponse wr = null;
                    Stream rs = null;
                    try
                    {
                        if (!HttpClient.PreparationUpload(request, blobs))
                        {
                            callback?.Invoke(HttpClientError.UnableToUploadBLOB, null, -1);
                        }
                        else
                        {
                            using (wr = (HttpWebResponse)request.GetResponse())
                            {
                                HttpClient.Fill(wr, response);
                                using (rs = wr.GetResponseStream())
                                {
                                    Func<byte[], int, bool> callbackt = null;
                                    if (callback != null)
                                    {
                                        callbackt = (buffer, len) =>
                                        {
                                            HttpClientError error = HttpClientError.Success;
                                            if (len < 0)
                                            {
                                                error = HttpClientError.ReadStreamInterruption;
                                            }
                                            return callback(error, buffer, len);
                                        };
                                    }
                                    success = HttpClient.CopyTo(rs, response, callbackt);
                                }
                            }
                        }
                        request.Abort();
                    }
                    catch (Exception)
                    {
                        if (wr != null && rs == null)
                        {
                            callback?.Invoke(HttpClientError.UnableToSendRequest, null, -1);
                        }
                        else if (rs != null)
                        {
                            callback?.Invoke(HttpClientError.UnableToGetResponseStream, null, -1);
                        }
                        else if (wr == null)
                        {
                            callback?.Invoke(HttpClientError.UnableOpenURL, null, -1);
                        }
                        if (rs != null)
                        {
                            rs.Close();
                        }
                        if (wr != null)
                        {
                            wr.Close();
                        }
                        if (request != null)
                        {
                            request.Abort();
                        }
                    }
                };
                if (!response.AsynchronousMode)
                {
                    copyto(response);
                }
                else
                {
                    (new Thread(copyto) { IsBackground = true, Priority = ThreadPriority.Lowest }).Start(response);
                }
                return success;
            }
            catch (Exception)
            {
                if (request != null)
                {
                    request.Abort();
                }
                return false;
            }
        }

        public static bool TryUpload(string url, IEnumerable<HttpPostValue> blobs, HttpClientOptions options, HttpClientResponse response)
        {
            if (options == null || response == null)
            {
                return false;
            }
            response.AsynchronousMode = false;
            return HttpClient.InternalTryUpload(url, blobs, options, response, null);
        }

        public static bool TryUploadAsync(string url, IEnumerable<HttpPostValue> blobs, HttpClientOptions options, HttpClientResponse response, HttpClientAsyncCallback callback)
        {
            if (options == null || response == null)
            {
                return false;
            }
            response.AsynchronousMode = true;
            return HttpClient.InternalTryUpload(url, blobs, options, response, callback);
        }
    }
}
