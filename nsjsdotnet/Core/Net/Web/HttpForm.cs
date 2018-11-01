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

            private static byte[] ReadLineAsBytes(Stream stream)
            {
                if (stream == null)
                {
                    return new byte[0];
                }
                using (MemoryStream out_stream = new MemoryStream())
                {
                    bool findrchar = true;
                    while (stream.Position < stream.Length)
                    {
                        int data = stream.ReadByte();
                        out_stream.WriteByte((byte)data);
                        if (data == '\r')
                        {
                            findrchar = true;
                        }
                        else if (data == '\n' && findrchar)
                        {
                            break;
                        }
                        else
                        {
                            findrchar = false;
                        }
                    }
                    out_stream.Position = 0;
                    return out_stream.ToArray();
                }
            }

            public static IList<HttpPostValue> GetValues(HttpRequest request)
            {
                IList<HttpPostValue> emtpry_post_values = new List<HttpPostValue>();
                if (request == null)
                {
                    return emtpry_post_values;
                }
                Stream upload_in_stream = request.InputStream;
                if (upload_in_stream == null)
                {
                    return emtpry_post_values;
                }
                string request_content_type = request.ContentType;
                if (request_content_type.Length < 20)
                {
                    return emtpry_post_values;
                }
                if (string.Compare(request_content_type.Substring(0, 20), "multipart/form-data;", true) != 0)
                {
                    return emtpry_post_values;
                }
                try
                {
                    List<HttpPostValue> out_post_values = new List<HttpPostValue>();
                    string boundary_code = string.Join(";", request.ContentType.Split(';').Skip(1)).Replace("boundary=", "").Trim();
                    byte[] chunk_boundary = Encoding.UTF8.GetBytes("--" + boundary_code + "\r\n");
                    byte[] end_boundary = Encoding.UTF8.GetBytes("--" + boundary_code + "--\r\n");
                    MemoryStream out_file_stream = new MemoryStream();
                    bool can_move_next = true;
                    HttpPostValue curr_data_value = null;
                    string mime_type_name = null;
                    while (can_move_next)
                    {
                        byte[] currentchunkbuf = ReadLineAsBytes(upload_in_stream);
                        if (currentchunkbuf == null | currentchunkbuf.Length <= 0)
                        {
                            break;
                        }
                        if (!(currentchunkbuf.Length == 2 && currentchunkbuf[0] == '\r' && currentchunkbuf[1] == '\n'))
                        {
                            out_file_stream.Write(currentchunkbuf, 0, currentchunkbuf.Length);
                        }
                        if (CompareBuffer(chunk_boundary, currentchunkbuf))
                        {
                            byte[] result = new byte[out_file_stream.Length - chunk_boundary.Length];
                            using (out_file_stream)
                            {
                                out_file_stream.Position = 0;
                                out_file_stream.Read(result, 0, result.Length);
                                can_move_next = true;
                                if (result.Length > 0)
                                {
                                    curr_data_value.Payload = result;
                                }
                                curr_data_value = new HttpPostValue();
                                out_post_values.Add(curr_data_value);
                            }
                            out_file_stream = new MemoryStream();
                        }
                        else if (Encoding.UTF8.GetString(currentchunkbuf).Contains("Content-Disposition"))
                        {
                            byte[] result = new byte[out_file_stream.Length - 2];
                            using (out_file_stream)
                            {
                                out_file_stream.Position = 0;
                                out_file_stream.Read(result, 0, result.Length);
                                can_move_next = true;
                                string[] disposition = Encoding.UTF8.GetString(result).Replace("Content-Disposition: form-data; name=\"", "").
                                    Replace("\"", "").Split(';');
                                curr_data_value.Name = disposition[0];
                                if (disposition.Length >= 2)
                                {
                                    string file = disposition[1];
                                    int index = file.IndexOf('=');
                                    if (index > -1)
                                    {
                                        file = file.Remove(0, index + 1);
                                    }
                                    curr_data_value.FileName = file;
                                }
                            }
                            out_file_stream = new MemoryStream();
                        }
                        else if ((mime_type_name = Encoding.UTF8.GetString(currentchunkbuf)).Contains("Content-Type"))
                        {
                            curr_data_value.ContentType = mime_type_name.Split(':')[1].Trim();
                            can_move_next = true;
                            using (out_file_stream)
                            {
                                curr_data_value.IsFileResource = 1;
                            }
                            out_file_stream = new MemoryStream();
                        }
                        else if (CompareBuffer(end_boundary, currentchunkbuf))
                        {
                            byte[] result = new byte[out_file_stream.Length - end_boundary.Length - 2];
                            using (out_file_stream)
                            {
                                out_file_stream.Position = 0;
                                out_file_stream.Read(result, 0, result.Length);
                                curr_data_value.Payload = result;
                            }
                            can_move_next = false;
                        }
                    }
                    return out_post_values;
                }
                catch (Exception)
                {
                    return emtpry_post_values;
                }
                finally
                {
                    upload_in_stream.Seek(0, SeekOrigin.Begin);
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
                                        if (len >= 2 && values.Count >= 2 &&
                                            payload[len - 2] == '\r' &&
                                            payload[len - 1] == '\n')
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
