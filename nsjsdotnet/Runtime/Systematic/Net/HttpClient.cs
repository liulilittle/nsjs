namespace nsjsdotnet.Runtime.Systematic.Net
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Collections.Specialized;
    using global::System.Net;
    using global::System.Net.Cache;
    using nsjsdotnet.Core.Net;
    using NSJSStream = nsjsdotnet.Runtime.Systematic.IO.Stream;
    using RESTClient = nsjsdotnet.Core.Net.HttpClient;

    static class HttpClient
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static HttpClient()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("TryDownload", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(TryDownload));
            owner.Set("TryDownloadAsync", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(TryDownloadAsync));
            owner.Set("TryUpload", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(TryUpload));
            owner.Set("TryUploadAsync", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(TryUploadAsync));
        }

        private static HttpClientOptions object2options(NSJSObject options)
        {
            if (options == null)
            {
                return null;
            }
            HttpClientOptions o = new HttpClientOptions();
            NSJSBoolean boolean = options.Get("AllowAutoRedirect") as NSJSBoolean;
            if (boolean != null)
            {
                o.AllowAutoRedirect = boolean.Value;
            }
            NSJSInt32 int32 = options.Get("AutomaticDecompression") as NSJSInt32;
            if (int32 != null)
            {
                o.AutomaticDecompression = (DecompressionMethods)int32.Value;
            }
            int32 = options.Get("CachePolicy") as NSJSInt32;
            if (int32 != null)
            {
                o.CachePolicy = new HttpRequestCachePolicy((HttpRequestCacheLevel)int32.Value);
            }
            int32 = options.Get("MaximumAutomaticRedirections") as NSJSInt32;
            if (int32 != null)
            {
                o.MaximumAutomaticRedirections = int32.Value;
            }
            int32 = options.Get("Timeout") as NSJSInt32;
            if (int32 != null)
            {
                o.Timeout = int32.Value;
            }
            NSJSString stringt = options.Get("Proxy") as NSJSString;
            if (stringt != null)
            {
                o.Proxy = new WebProxy(stringt.Value);
            }
            stringt = options.Get("Referer") as NSJSString;
            if (stringt != null)
            {
                o.Referer = stringt.Value;
            }
            NSJSInt32Array int32array = options.Get("Range") as NSJSInt32Array;
            if (int32array != null)
            {
                o.Range = int32array.Buffer;
            }
            NSJSArray array = options.Get("Headers") as NSJSArray;
            if (array != null)
            {
                NameValueCollection headers = o.Headers;
                int count = array.Length;
                for (int i = 0; i < count; i++)
                {
                    stringt = array[i] as NSJSString;
                    if (stringt == null)
                    {
                        continue;
                    }
                    headers.Add(headers);
                }
            }
            return o;
        }

        private static HttpClientResponse object2response(NSJSObject response)
        {
            if (response == null)
            {
                return null;
            }
            HttpClientResponse o = new HttpClientResponse();
            NSJSBoolean boolean = response.Get("ManualWriteToStream") as NSJSBoolean;
            if (boolean != null)
            {
                o.ManualWriteToStream = boolean.Value;
            }
            NSJSObject objectt = response.Get("ContentStream") as NSJSObject;
            if (objectt != null)
            {
                o.ContentStream = NSJSStream.Get(objectt);
            }
            return o;
        }

        private static IEnumerable<HttpPostValue> array2blobs(NSJSArray array)
        {
            if (array == null)
            {
                return null;
            }
            IList<HttpPostValue> blobs = new List<HttpPostValue>();
            int count = array.Length;
            for (int i = 0; i < count; i++)
            {
                NSJSObject o = array[i] as NSJSObject;
                if (o == null)
                {
                    continue;
                }
                HttpPostValue blob = new HttpPostValue();
                blob.FileName = (o.Get("FileName") as NSJSString)?.Value;
                NSJSBoolean boolean = o.Get("IsText") as NSJSBoolean;
                if (boolean != null)
                {
                    blob.IsText = boolean.Value;
                }
                blob.Key = (o.Get("Key") as NSJSString)?.Value;
                blob.Value = (o.Get("Value") as NSJSUInt8Array)?.Buffer;
            }
            return blobs;
        }

        private static bool fill2object(NSJSObject o, HttpClientResponse responset)
        {
            if (o == null || responset == null)
            {
                return false;
            }
            NSJSVirtualMachine machine = o.VirtualMachine;
            machine.Join((sender, state) =>
            {
                o.Set("AsynchronousMode", responset.AsynchronousMode);
                o.Set("CharacterSet", responset.CharacterSet ?? string.Empty);
                o.Set("ContentLength", responset.ContentLength);
                o.Set("ContentType", responset.ContentType ?? string.Empty);
                o.Set("LastModified", NSJSDateTime.Invalid(responset.LastModified) ? NSJSDateTime.Min :
                    responset.LastModified);
                o.Set("ManualWriteToStream", responset.ManualWriteToStream);
                o.Set("ResponseUri", responset.ResponseUri == null ? string.Empty :
                    responset.ResponseUri.ToString());
                o.Set("Server", responset.Server ?? string.Empty);
                o.Set("StatusCode", unchecked((int)responset.StatusCode));
                o.Set("ContentEncoding", responset.ContentEncoding ?? string.Empty);
                o.Set("StatusDescription", responset.StatusDescription ?? string.Empty);
            });
            return true;
        }

        // [native] bool HttpClient.TryDownload(string url, HttpClientOptions options, HttpClientResponse response)
        private static void TryDownload(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            if (arguments.Length > 2)
            {
                string url = (arguments[0] as NSJSString)?.Value;
                HttpClientOptions options = HttpClient.object2options(arguments[1] as NSJSObject);
                HttpClientResponse response = HttpClient.object2response(arguments[2] as NSJSObject);
                success = RESTClient.TryDownload(url, options, response);
            }
            arguments.SetReturnValue(success);
        }

        // [native] bool HttpClient.TryDownloadAsync(string url, HttpClientOptions options, HttpClientResponse response, HttpClientAsyncCallback callback)
        private static void TryDownloadAsync(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            if (arguments.Length > 3)
            {
                string url = (arguments[0] as NSJSString)?.Value;
                HttpClientOptions options = HttpClient.object2options(arguments[1] as NSJSObject);
                NSJSObject response = arguments[2] as NSJSObject;
                if (options != null && response != null)
                {
                    NSJSFunction callback = arguments[3] as NSJSFunction;
                    if (callback != null)
                    {
                        callback.CrossThreading = true;
                    }
                    response.CrossThreading = true;
                    bool fillToObject = false;
                    HttpClientResponse responset = HttpClient.object2response(response);
                    success = RESTClient.TryDownloadAsync(url, options, responset, (error, buffer, count) =>
                    {
                        NSJSVirtualMachine machine = arguments.VirtualMachine;
                        if (error == HttpClientError.Success && !fillToObject)
                        {
                            fillToObject = true;
                            fill2object(response, responset);
                        }
                        if (callback != null)
                        {
                            bool breakto = false;
                            machine.Join((sender, state) => breakto = ((callback.Call
                            (
                                NSJSInt32.New(machine, (int)error),
                                NSJSValue.NullMerge(machine, buffer != null && count >= 0 ? NSJSUInt8Array.New(machine, buffer, count) : null)
                            ) as NSJSBoolean)?.Value) == false);
                            if (breakto)
                            {
                                return false;
                            }
                        }
                        return count > 0;
                    });
                }
            }
            arguments.SetReturnValue(success);
        }

        // [native] bool HttpClient.TryUpload(string url, HttpPostValue[] blobs, HttpClientOptions options, HttpClientResponse response)
        private static void TryUpload(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            if (arguments.Length > 3)
            {
                string url = (arguments[0] as NSJSString)?.Value;
                IEnumerable<HttpPostValue> blobs = HttpClient.array2blobs(arguments[1] as NSJSArray);
                HttpClientOptions options = HttpClient.object2options(arguments[2] as NSJSObject);
                HttpClientResponse response = HttpClient.object2response(arguments[3] as NSJSObject);
                success = RESTClient.TryUpload(url, blobs, options, response);
            }
            arguments.SetReturnValue(success);
        }

        // [native] bool HttpClient.TryUploadAsync(string url, HttpPostValue[] blobs, HttpClientOptions options, HttpClientResponse response, HttpClientAsyncCallback callback)
        private static void TryUploadAsync(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            if (arguments.Length > 4)
            {
                string url = (arguments[0] as NSJSString)?.Value;
                HttpClientOptions options = HttpClient.object2options(arguments[2] as NSJSObject);
                NSJSObject response = arguments[3] as NSJSObject;
                IEnumerable<HttpPostValue> blobs = HttpClient.array2blobs(arguments[1] as NSJSArray);
                if (options != null && response != null)
                {
                    NSJSFunction callback = arguments[4] as NSJSFunction;
                    if (callback != null)
                    {
                        callback.CrossThreading = true;
                    }
                    response.CrossThreading = true;
                    bool fillToObject = false;
                    HttpClientResponse responset = HttpClient.object2response(response);
                    success = RESTClient.TryUploadAsync(url, blobs, options, responset, (error, buffer, count) =>
                    {
                        NSJSVirtualMachine machine = arguments.VirtualMachine;
                        if (error == HttpClientError.Success && !fillToObject)
                        {
                            fillToObject = true;
                            fill2object(response, responset);
                        }
                        if (callback != null)
                        {
                            bool breakto = false;
                            machine.Join((sender, state) => breakto = ((callback.Call
                            (
                                NSJSInt32.New(machine, (int)error),
                                NSJSValue.NullMerge(machine, buffer != null && count >= 0 ? NSJSUInt8Array.New(machine, buffer, count) : null)
                            ) as NSJSBoolean)?.Value) == false);
                            if (breakto)
                            {
                                return false;
                            }
                        }
                        return count > 0;
                    });
                }
            }
            arguments.SetReturnValue(success);
        }
    }
}
