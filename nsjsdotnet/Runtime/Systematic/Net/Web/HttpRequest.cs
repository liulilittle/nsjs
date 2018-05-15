namespace nsjsdotnet.Runtime.Systematic.Net.Web
{
    using nsjsdotnet.Core;
    using System;
    using HTTPRequest = nsjsdotnet.Core.Net.Web.HttpRequest;
    using NSJSEncoding = nsjsdotnet.Runtime.Systematic.Text.Encoding;
    using NSJSStream = nsjsdotnet.Runtime.Systematic.IO.Stream;

    sealed class HttpRequest
    {
        public static bool Close(NSJSValue request)
        {
            NSJSObject o = request as NSJSObject;
            if (o == null)
            {
                return false;
            }
            ObjectAuxiliary.RemoveInKeyValueCollection(o.Get("InputStream") as NSJSObject);
            NSJSArray files = o.Get("Files") as NSJSArray;
            if (files != null)
            {
                foreach (NSJSValue value in files)
                {
                    if (value == null)
                    {
                        continue;
                    }
                    NSJSObject posedfile = value as NSJSObject;
                    if (posedfile == null)
                    {
                        continue;
                    }
                    ObjectAuxiliary.RemoveInKeyValueCollection(o.Get("InputStream") as NSJSObject);
                }
            }
            return ObjectAuxiliary.RemoveInKeyValueCollection(request as NSJSObject);
        }

        public static NSJSObject New(NSJSVirtualMachine machine, NSJSObject context, HTTPRequest request)
        {
            if (machine == null || context == null || request == null)
            {
                return null;
            }
            NSJSObject objective = NSJSObject.New(machine);
            objective.Set("HttpMethod", request.HttpMethod);
            objective.Set("IsLocal", request.IsLocal);
            objective.Set("KeepAlive", request.KeepAlive);
            objective.Set("ContentType", request.ContentType);
            objective.Set("CurrentContext", context);
            objective.Set("Files", ObjectAuxiliary.ToObject(machine, request.Files));
            objective.Set("InputStream", NSJSValue.NullMerge(machine, NSJSStream.New(machine, request.InputStream)));
            objective.Set("Cookies", ArrayAuxiliary.ToArray(machine, request.Cookies));
            objective.Set("RequestTraceIdentifier", request.RequestTraceIdentifier.ToString()); // "D"
            objective.Set("RemoteEndPoint", ObjectAuxiliary.ToObject(machine, request.RemoteEndPoint));
            objective.Set("ContentEncoding", NSJSEncoding.New(machine, request.ContentEncoding ?? NSJSEncoding.DefaultEncoding));
            objective.Set("Form", ObjectAuxiliary.ToObject(machine, request.Form));
            objective.Set("QueryString", ObjectAuxiliary.ToObject(machine, request.QueryString));
            objective.Set("ContentLength", Convert.ToDouble(request.ContentLength));
            objective.Set("AcceptTypes", ArrayAuxiliary.ToArray(machine, request.AcceptTypes));
            objective.Set("Path", request.Path);
            objective.Set("RawUrl", request.RawUrl);
            objective.Set("ServiceName", request.ServiceName);
            objective.Set("Url", request.Url?.ToString());
            objective.Set("UrlReferrer", request.UrlReferrer?.ToString());
            objective.Set("UserAgent", request.UserAgent);
            objective.Set("UserHostAddress", request.UserHostAddress);
            objective.Set("UserHostName", request.UserHostName);
            objective.Set("ProtocolVersion", request.ProtocolVersion.ToString());
            objective.Set("LocalEndPoint", ObjectAuxiliary.ToObject(machine, request.LocalEndPoint));
            objective.Set("UserLanguages", ArrayAuxiliary.ToArray(machine, request.UserLanguages));
            return objective;
        }
    }
}
