namespace nsjsdotnet.Runtime.Systematic.Net.Web
{
    using nsjsdotnet.Core;
    using System;
    using System.Net;
    using HTTPResponse = nsjsdotnet.Core.Net.Web.HttpResponse;
    using NSJSEncoding = nsjsdotnet.Runtime.Systematic.Text.Encoding;

    sealed class HttpResponse
    {
        private static readonly NSJSFunctionCallback g_ContentEncodingProc;
        private static readonly NSJSFunctionCallback g_ContentTypeProc;
        private static readonly NSJSFunctionCallback g_StatusDescriptionProc;
        private static readonly NSJSFunctionCallback g_StatusCodeProc;
        private static readonly NSJSFunctionCallback g_KeepAliveProc;
        private static readonly NSJSFunctionCallback g_ProtocolVersionProc;
        private static readonly NSJSFunctionCallback g_RedirectLocationProc;
        private static readonly NSJSFunctionCallback g_SendChunkedProc;
        private static readonly NSJSFunctionCallback g_HeadersProc;
        private static readonly NSJSFunctionCallback g_RedirectProc;
        private static readonly NSJSFunctionCallback g_WriteProc;
        private static readonly NSJSFunctionCallback g_WriteFileProc;
        private static readonly NSJSFunctionCallback g_BinaryWriteProc;
        private static readonly NSJSFunctionCallback g_EndProc;
        private static readonly NSJSFunctionCallback g_AbortProc;
        private static readonly NSJSFunctionCallback g_SetCookieProc;
        private static readonly NSJSFunctionCallback g_AddHeaderProc;
        private static readonly NSJSFunctionCallback g_CookiesProc;

        static HttpResponse()
        {
            g_ContentEncodingProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ContentEncoding);
            g_ContentTypeProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ContentType);
            g_StatusDescriptionProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(StatusDescription);
            g_StatusCodeProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(StatusCode);
            g_KeepAliveProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(KeepAlive);
            g_ProtocolVersionProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ProtocolVersion);
            g_RedirectLocationProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(RedirectLocation);
            g_SendChunkedProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SendChunked);
            g_HeadersProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Headers);
            g_RedirectProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Redirect);
            g_WriteProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Write);
            g_WriteFileProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(WriteFile);
            g_BinaryWriteProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(BinaryWrite);
            g_EndProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(End);
            g_AbortProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Abort);
            g_SetCookieProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SetCookie);
            g_AddHeaderProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(AddHeader);
            g_CookiesProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Cookies);
        }

        public static NSJSObject New(NSJSVirtualMachine machine, NSJSObject context, HTTPResponse response)
        {
            if (machine == null || context == null || response == null)
            {
                return null;
            }
            NSJSObject objective = NSJSObject.New(machine);
            objective.Set("CurrentContext", context);
            objective.Set("ContentEncoding", g_ContentEncodingProc);
            objective.Set("ContentType", g_ContentTypeProc);
            objective.Set("StatusDescription", g_StatusDescriptionProc);
            objective.Set("StatusCode", g_StatusCodeProc);
            objective.Set("KeepAlive", g_KeepAliveProc);
            objective.Set("ProtocolVersion", g_ProtocolVersionProc);
            objective.Set("RedirectLocation", g_RedirectLocationProc);
            objective.Set("SendChunked", g_SendChunkedProc);
            objective.Set("Headers", g_HeadersProc);
            objective.Set("Cookies", g_CookiesProc);

            objective.Set("Redirect", g_RedirectProc);
            objective.Set("End", g_EndProc);
            objective.Set("Abort", g_AbortProc);

            objective.Set("SetCookie", g_SetCookieProc);
            objective.Set("AppendCookie", g_SetCookieProc);
            objective.Set("AddHeader", g_AddHeaderProc);
            objective.Set("AppendHeader", g_AddHeaderProc);

            objective.Set("Write", g_WriteProc);
            objective.Set("WriteFile", g_WriteFileProc);
            objective.Set("BinaryWrite", g_BinaryWriteProc);
            NSJSKeyValueCollection.Set(objective, response);
            return objective;
        }

        private static void Cookies(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response, arguments) => 
                arguments.SetReturnValue(ArrayAuxiliary.ToArray(arguments.VirtualMachine, response.Cookies)),
                (response, arguments, value) => ArrayAuxiliary.Fill(value, response.Cookies));
        }

        private static void SetCookie(IntPtr info)
        {
            ObjectAuxiliary.Call(info, (HTTPResponse response, NSJSFunctionCallbackInfo arguments, NSJSValue solt0) =>
            {
                bool success = false;
                Cookie cookie = ObjectAuxiliary.ToCookie(solt0);
                if (cookie != null)
                {
                    success = response.SetCookie(cookie);
                }
                arguments.SetReturnValue(success);
            });
        }

        private static void AddHeader(IntPtr info)
        {
            ObjectAuxiliary.Call(info, (HTTPResponse response, NSJSFunctionCallbackInfo arguments, NSJSValue solt0) =>
            {
                string value = ValueAuxiliary.ToString(arguments.Length > 1 ? arguments[1] : null);
                string name = ValueAuxiliary.ToString(solt0);
                arguments.SetReturnValue(response.AppendHeader(name, value));
            });
        }

        private static void End(IntPtr info)
        {
            ObjectAuxiliary.Call<HTTPResponse>(info, (response, arguments) => response.End());
        }

        private static void Abort(IntPtr info)
        {
            ObjectAuxiliary.Call<HTTPResponse>(info, (response, arguments) => response.Abort());
        }

        private static void Write(IntPtr info)
        {
            ObjectAuxiliary.Call<HTTPResponse>(info, (response, arguments, value) => arguments.SetReturnValue(response.Write(value)));
        }

        private static void BinaryWrite(IntPtr info)
        {
            ObjectAuxiliary.Call<HTTPResponse>(info, (response, arguments, value) =>
            {
                NSJSInt32 count = null;
                NSJSInt32 offset = null;
                if (arguments.Length == 2)
                {
                    count = arguments[1] as NSJSInt32;
                }
                else if (arguments.Length > 2)
                {
                    offset = arguments[1] as NSJSInt32;
                    count = arguments[2] as NSJSInt32;
                }
                int ofs = offset == null ? 0 : offset.Value;
                int size = count == null ? value.Length : count.Value;
                if (ofs < 0)
                {
                    ofs = 0;
                }
                if (size < 0)
                {
                    size = 0;
                }
                arguments.SetReturnValue(response.BinaryWrite(value));
            });
        }

        private static void WriteFile(IntPtr info)
        {
            ObjectAuxiliary.Call<HTTPResponse>(info, (response, arguments, value) =>
            {
                NSJSInt32 count = null;
                NSJSInt32 offset = null;
                if (arguments.Length == 2)
                {
                    count = arguments[1] as NSJSInt32;
                }
                else if (arguments.Length > 2)
                {
                    offset = arguments[1] as NSJSInt32;
                    count = arguments[2] as NSJSInt32;
                }
                int ofs = offset == null ? 0 : offset.Value;
                int size = count == null ? value.Length : count.Value;
                if (ofs < 0)
                {
                    ofs = 0;
                }
                if (size < 0)
                {
                    size = 0;
                }
                arguments.SetReturnValue(response.WriteFile(value, ofs, size));
            });
        }

        public static bool Close(NSJSObject response)
        {
            return ObjectAuxiliary.RemoveInKeyValueCollection(response);
        }

        private static void Redirect(IntPtr info)
        {
            ObjectAuxiliary.Call<HTTPResponse>(info, (response, arguments, value) => response.Redirect(value));
        }

        private static void Headers(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response, arguments) => arguments.SetReturnValue(ObjectAuxiliary.
                ToObject(arguments.VirtualMachine, response.Headers)),
            (response, arguments, objecttive) => ObjectAuxiliary.Fill(objecttive, response.Headers));
        }

        private static void SendChunked(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response) => response.SendChunked, (response, value) => response.SendChunked = value);
        }

        private static void RedirectLocation(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response) => response.RedirectLocation, (response, value) => response.RedirectLocation = value);
        }

        private static void ProtocolVersion(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response) => response.ProtocolVersion?.ToString(), (response, value) => response.ProtocolVersion?.ToString());
        }

        private static void StatusCode(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response) => response.StatusCode, (response, value) => response.StatusCode = value);
        }

        private static void KeepAlive(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response) => response.KeepAlive, (response, value) => response.KeepAlive = value);
        }

        private static void StatusDescription(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response) => response.StatusDescription, (response, value) => response.StatusDescription = value);
        }

        private static void ContentType(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<HTTPResponse>(info, (response) => response.ContentType, (response, value) => response.ContentType = value);
        }

        public static HTTPResponse GetResponse(NSJSObject response)
        {
            if (response == null)
            {
                return null;
            }
            return NSJSKeyValueCollection.Get<HTTPResponse>(response);
        }

        private static void ContentEncoding(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            HTTPResponse response = GetResponse(arguments.This);
            if (response == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                NSJSVirtualMachine machine = arguments.VirtualMachine;
                if (arguments.Length <= 0)
                {
                    arguments.SetReturnValue(NSJSEncoding.New(machine, response.ContentEncoding));
                }
                else
                {
                    var encoding = NSJSEncoding.GetEncoding(arguments[0] as NSJSObject);
                    if (encoding == null)
                    {
                        encoding = NSJSEncoding.DefaultEncoding;
                    }
                    response.ContentEncoding = encoding;
                    arguments.SetReturnValue(true);
                }
            }
        }
    }
}
