namespace nsjsdotnet.Core.Net.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;

    class HttpForm
    {
        public readonly NameValueCollection Form = null;
        public readonly HttpFileCollection Files = null;

        public HttpForm(NameValueCollection form, HttpFileCollection files)
        {
            this.Form = form;
            this.Files = files;
        }

        class HttpPostValue
        {
            /// <summary>  
            /// 0=> 参数  
            /// 1=> 文件  
            /// </summary>  
            public int IsFileResource = 0;
            public string Name;
            public byte[] Payload;
            public string FileName;
            public string ContentType;

            private static bool CompareBuffer(byte[] source, byte[] comparison)
            {
                try
                {
                    int count = source.Length;
                    if (source.Length != comparison.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        if (source[i] != comparison[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private static byte[] ReadLineAsBytes(Stream SourceStream)
            {
                var resultStream = new MemoryStream();
                while (true)
                {
                    int data = SourceStream.ReadByte();
                    resultStream.WriteByte((byte)data);
                    if (data == 10)
                        break;
                }
                resultStream.Position = 0;
                byte[] dataBytes = new byte[resultStream.Length];
                resultStream.Read(dataBytes, 0, dataBytes.Length);
                return dataBytes;
            }

            public static IList<HttpPostValue> GetValues(HttpRequest request)
            {
                try
                {
                    List<HttpPostValue> HttpListenerPostValueList = new List<HttpPostValue>();
                    if (request.ContentType.Length > 20 && string.Compare(request.ContentType.Substring(0, 20), "multipart/form-data;", true) == 0)
                    {
                        string[] HttpListenerPostValue = request.ContentType.Split(';').Skip(1).ToArray();
                        string boundary = string.Join(";", HttpListenerPostValue).Replace("boundary=", "").Trim();
                        byte[] ChunkBoundary = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
                        byte[] EndBoundary = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");
                        Stream SourceStream = request.InputStream;
                        MemoryStream resultStream = new MemoryStream();
                        bool CanMoveNext = true;
                        HttpPostValue data = null;
                        string ContentType = null;
                        while (CanMoveNext)
                        {
                            byte[] currentChunk = ReadLineAsBytes(SourceStream);
                            if (!Encoding.UTF8.GetString(currentChunk).Equals("\r\n"))
                            {
                                resultStream.Write(currentChunk, 0, currentChunk.Length);
                            }
                            if (CompareBuffer(ChunkBoundary, currentChunk))
                            {
                                byte[] result = new byte[resultStream.Length - ChunkBoundary.Length];
                                resultStream.Position = 0;
                                resultStream.Read(result, 0, result.Length);
                                CanMoveNext = true;
                                if (result.Length > 0)
                                {
                                    data.Payload = result;
                                }
                                data = new HttpPostValue();
                                HttpListenerPostValueList.Add(data);
                                resultStream.Dispose();
                                resultStream = new MemoryStream();

                            }
                            else if (Encoding.UTF8.GetString(currentChunk).Contains("Content-Disposition"))
                            {
                                byte[] result = new byte[resultStream.Length - 2];
                                resultStream.Position = 0;
                                resultStream.Read(result, 0, result.Length);
                                CanMoveNext = true;
                                string[] disposition = Encoding.UTF8.GetString(result).Replace("Content-Disposition: form-data; name=\"", "").
                                    Replace("\"", "").Split(';');
                                data.Name = disposition[0];
                                if (disposition.Length >= 2)
                                {
                                    string file = disposition[1];
                                    int index = file.IndexOf('=');
                                    if (index > -1)
                                        file = file.Remove(0, index + 1);
                                    data.FileName = file;
                                }
                                resultStream.Dispose();
                                resultStream = new MemoryStream();
                            }
                            else if ((ContentType = Encoding.UTF8.GetString(currentChunk)).Contains("Content-Type"))
                            {
                                CanMoveNext = true;
                                data.ContentType = ContentType.Split(':')[1].Trim();
                                data.IsFileResource = 1;
                                resultStream.Dispose();
                                resultStream = new MemoryStream();
                            }
                            else if (CompareBuffer(EndBoundary, currentChunk))
                            {
                                byte[] result = new byte[resultStream.Length - EndBoundary.Length - 2];
                                resultStream.Position = 0;
                                resultStream.Read(result, 0, result.Length);
                                data.Payload = result;
                                resultStream.Dispose();
                                CanMoveNext = false;
                            }
                        }
                    }
                    return HttpListenerPostValueList;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static HttpForm Resolve(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            NameValueCollection forms = new NameValueCollection();
            HttpFileCollection files = new HttpFileCollection();
            if (request.ContentLength > 0)
            {
                string method = request.HttpMethod;
                if (!string.IsNullOrEmpty(method) && method.ToUpper().Contains("POST"))
                {
                    string type = request.ContentType;
                    if (!string.IsNullOrEmpty(type))
                    {
                        type = type.ToLower();
                        if (type.Contains("application/x-www-form-urlencoded"))
                        {
                            forms = LoadUrlencoded(request);
                        }
                        else
                        {
                            IList<HttpPostValue> values = HttpPostValue.GetValues(request);
                            request.InputStream.Seek(0, SeekOrigin.Begin);
                            if (values != null)
                            {
                                foreach (HttpPostValue value in values)
                                {
                                    if (value.IsFileResource == 0)
                                    {
                                        byte[] payload = value.Payload;
                                        int len = payload.Length;
                                        if (values.Count >= 2 && payload[len - 2] == '\r' && payload[len - 1] == '\n')
                                        {
                                            len -= 2;
                                        }
                                        string content = Encoding.UTF8.GetString(payload, 0, len);
                                        forms.Add(value.Name, content);
                                    }
                                    else
                                    {
                                        HttpPostedFile file = new HttpPostedFile();
                                        file.FileName = value.FileName;
                                        file.InputStream = new MemoryStream(value.Payload);
                                        file.ContentLength = value.Payload.Length;
                                        file.ContentType = value.ContentType;
                                        files.Add(value.Name, file);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (request.HttpMethod.ToUpper().Contains("GET") && !string.IsNullOrEmpty(request.RawUrl.Replace((request.Path + "?"), "")))
            {
                string[] url = request.RawUrl.Replace((request.Path + "?"), "").Split('&');
                if (url.Length > 0)
                {
                    foreach (string item in url)
                    {
                        if (string.IsNullOrEmpty(item))
                        {
                            continue;
                        }
                        string[] kv = item.Split('=');
                        if (kv.Length == 2)
                        {
                            forms.Add(kv[0], HttpUtility.UrlDecode(kv[1]));
                        }
                    }
                }
            }

            return new HttpForm(forms, files);
        }

        private static NameValueCollection LoadUrlencoded(HttpRequest request)
        {
            if (request.ContentLength <= 0)
            {
                return new NameValueCollection();
            }
            Stream s = request.InputStream;
            StreamReader sr = new StreamReader(s, request.ContentEncoding);
            string body = HttpUtility.UrlDecode(sr.ReadToEnd());
            s.Seek(0, SeekOrigin.Begin);
            return HttpUtility.ParseQueryString(body);
        }
    }
}
