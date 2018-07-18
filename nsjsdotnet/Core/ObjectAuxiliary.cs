namespace nsjsdotnet.Core
{
    using nsjsdotnet.Core.Data.Database;
    using nsjsdotnet.Core.Net.Web;
    using nsjsdotnet.Core.Utilits;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using NSJSStream = nsjsdotnet.Runtime.Systematic.IO.Stream;

    public static class ObjectAuxiliary
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        public static NSJSValue SendEvent(NSJSObject obj, string evt)
        {
            return SendEvent(obj, evt, null);
        }

        public static NSJSValue SendEvent(NSJSObject obj, string evt, params NSJSValue[] args)
        {
            return SendEvent(obj, evt, (IEnumerable<NSJSValue>)args);
        }

        public static NSJSValue SendEvent(NSJSObject obj, string evt, IEnumerable<NSJSValue> args)
        {
            if (obj == null || string.IsNullOrEmpty(evt))
            {
                return null;
            }
            NSJSFunction callback = obj.Get(evt) as NSJSFunction;
            if (callback == null)
            {
                return null;
            }
            if (args == null)
            {
                return callback.Call();
            }
            return callback.Call(args);
        }

        public static bool RemoveInKeyValueCollection(NSJSObject value)
        {
            if (value == null)
            {
                return false;
            }
            object result;
            return NSJSKeyValueCollection.Release(value, out result);
        }

        public static Cookie ToCookie(NSJSValue value)
        {
            NSJSObject o = value as NSJSObject;
            if (o == null)
            {
                return null;
            }
            Cookie cookie = new Cookie();
            cookie.Comment = ValueAuxiliary.ToString(o.Get("Comment"));
            cookie.Discard = ValueAuxiliary.ToBoolean(o.Get("Discard"));
            Uri url = default(Uri);
            if (Uri.TryCreate(ValueAuxiliary.ToString(o.Get("CommentUri")), UriKind.RelativeOrAbsolute, out url))
            {
                cookie.CommentUri = url;
            }
            cookie.Domain = ValueAuxiliary.ToString(o.Get("Domain"));
            cookie.Expired = ValueAuxiliary.ToBoolean(o.Get("Expired"));
            cookie.Expires = ValueAuxiliary.ToDateTime(o.Get("Expires"));
            cookie.HttpOnly = ValueAuxiliary.ToBoolean(o.Get("HttpOnly"));
            cookie.Name = ValueAuxiliary.ToString(o.Get("Name"));
            cookie.Path = ValueAuxiliary.ToString(o.Get("Path"));
            cookie.Port = ValueAuxiliary.ToString(o.Get("Port"));
            cookie.Secure = ValueAuxiliary.ToBoolean(o.Get("Secure"));
            cookie.Value = ValueAuxiliary.ToString(o.Get("Value"));
            cookie.Version = ValueAuxiliary.ToInt32(o.Get("Version"));
            return cookie;
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo> get, Action<TThis, NSJSFunctionCallbackInfo, NSJSObject> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, get, (TThis self, NSJSFunctionCallbackInfo arguments, NSJSValue value) => set(self, arguments, value as NSJSObject));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, bool> get, Action<TThis, bool> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, ValueAuxiliary.ToBoolean(value)));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, uint> get, Action<TThis, uint> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, ValueAuxiliary.ToUInt32(value)));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, DateTime> get, Action<TThis, DateTime> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, ValueAuxiliary.ToDateTime(value)));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, double> get, Action<TThis, double> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, ValueAuxiliary.ToDouble(value)));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, int> get, Action<TThis, int> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, ValueAuxiliary.ToInt32(value)));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, byte[]> get, Action<TThis, byte[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSUInt8Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, sbyte[]> get, Action<TThis, sbyte[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSInt8Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, ushort[]> get, Action<TThis, ushort[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSUInt16Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, short[]> get, Action<TThis, short[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSInt16Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, uint[]> get, Action<TThis, uint[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSUInt32Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, int[]> get, Action<TThis, int[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSInt32Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, float[]> get, Action<TThis, float[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSFloat32Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, double[]> get, Action<TThis, double[]> set)
        {
            Throwable.ArgumentNullException(info, get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSFloat64Array)?.Buffer));
        }

        public static void GetOrSetProperty<TThis>(IntPtr info, Func<TThis, string> get, Action<TThis, string> set)
        {
            Throwable.ArgumentNullException(get, set);
            GetOrSetProperty<TThis>(info, (self, arguments) => arguments.SetReturnValue(get(self)), (self, value) => set(self, (value as NSJSString)?.Value));
        }

        private static void GetOrSetProperty<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo> get, Action<TThis, NSJSValue> set)
        {
            if (get == null)
            {
                throw new ArgumentNullException("get");
            }
            GetOrSetProperty<TThis>(info, get, (self, arguments, value) => set(self, value));
        }

        private static void GetOrSetProperty<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo> get, Action<TThis, NSJSFunctionCallbackInfo, NSJSValue> set)
        {
            if (info == NULL)
            {
                throw new ArgumentNullException("info");
            }
            if (get == null)
            {
                throw new ArgumentNullException("get");
            }
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            TThis response = NSJSKeyValueCollection.Get<TThis>(arguments.This);
            if (response == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                NSJSVirtualMachine machine = arguments.VirtualMachine;
                if (arguments.Length <= 0)
                {
                    get(response, arguments);
                }
                else
                {
                    try
                    {
                        set(response, arguments, arguments[0]);
                        arguments.SetReturnValue(true);
                    }
                    catch (Exception exception)
                    {
                        Throwable.Exception(arguments.VirtualMachine, exception);
                    }
                }
            }
        }

        public static MailMessage ToMailMessage(NSJSValue value)
        {
            NSJSObject mail = value as NSJSObject;
            if (mail == null)
            {
                return null;
            }
            MailMessage message = new MailMessage();
            message.Body = ValueAuxiliary.ToString(mail.Get("Body"));
            message.Subject = ValueAuxiliary.ToString(mail.Get("Subject"));
            message.IsBodyHtml = ValueAuxiliary.ToBoolean(mail.Get("IsBodyHtml"));
            message.BodyEncoding = Encoding.UTF8;
            message.HeadersEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;
            MailAddress address = ToMailAddress(mail.Get("From"));
            if (address == null)
            {
                return null;
            }
            message.From = address;
            message.Sender = ToMailAddress(mail.Get("Sender"));
            ArrayAuxiliary.Fill(mail.Get("To"), message.To);
            ArrayAuxiliary.Fill(mail.Get("ReplyToList"), message.ReplyToList);
            ArrayAuxiliary.Fill(mail.Get("Attachments"), message.Attachments);
            return message;
        }

        public static Attachment ToAttachment(NSJSValue value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is NSJSString)
            {
                string path = ((NSJSString)value).Value;
                if (!File.Exists(path))
                {
                    return null;
                }
                return new Attachment(path);
            }
            if (value is NSJSObject)
            {
                NSJSObject o = (NSJSObject)value;
                string fileName = o.Get("FileName").As<string>();
                string mediaType = o.Get("MediaType").As<string>();
                string blobName = o.Get("Name").As<string>();
                Stream contentStream = NSJSStream.Get(o.Get("ContentStream") as NSJSObject);
                if (contentStream != null && !string.IsNullOrEmpty(blobName))
                {
                    return new Attachment(contentStream, blobName, mediaType);
                }
                if (!File.Exists(fileName))
                {
                    return null;
                }
                return new Attachment(fileName, mediaType);
            }
            return null;
        }

        public static MailAddress ToMailAddress(NSJSValue value)
        {
            NSJSObject a = value as NSJSObject;
            string address = null;
            if (a == null)
            {
                NSJSString s = value as NSJSString;
                if (s != null)
                {
                    address = s.Value;
                    if (string.IsNullOrEmpty(address))
                    {
                        return null;
                    }
                    return new MailAddress(address);
                }
                return null;
            }
            address = ValueAuxiliary.ToString(a.Get("Address"));
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }
            string displayName = ValueAuxiliary.ToString(a.Get("DisplayName"));
            return new MailAddress(address, displayName, Encoding.UTF8);
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, bool> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, ValueAuxiliary.ToBoolean(value)));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, byte[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSUInt8Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, sbyte[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSInt8Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, ushort[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSUInt16Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, short[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSInt16Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, int[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSInt32Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, uint[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSUInt32Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, float[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSFloat32Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, double[]> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSFloat64Array)?.Buffer));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, int> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, ValueAuxiliary.ToInt32(value)));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, uint> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, ValueAuxiliary.ToUInt32(value)));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, double> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, ValueAuxiliary.ToDouble(value)));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, DateTime> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, ValueAuxiliary.ToDateTime(value)));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, string> doo)
        {
            Throwable.ArgumentNullException(info, doo);
            Call<TThis>(info, (self, arguments, value) => doo(self, arguments, (value as NSJSString)?.Value));
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo, NSJSValue> doo)
        {
            Func<TThis, NSJSFunctionCallbackInfo, NSJSValue, NSJSValue> callback = null;
            if (doo != null)
            {
                callback = (self, arguments, value) =>
                {
                    doo(self, arguments, value);
                    return NSJSValue.Null(arguments.VirtualMachine);
                };
            }
            Call<TThis>(info, callback);
        }

        public static IDbCommand ToDbCommand(DatabaseAccessAdapter adapter, string text, IEnumerable<NSJSValue> arguments)
        {
            return ToDbCommand(adapter, text, 0, arguments);
        }

        public static IDbCommand ToDbCommand(DatabaseAccessAdapter adapter, string text,
            int startIndex, IEnumerable<NSJSValue> arguments)
        {
            if (startIndex < 0 || adapter == null || string.IsNullOrEmpty(text))
            {
                return null;
            }
            IDbCommand command = adapter.CreateCommand();
            command.CommandText = text;
            IDataParameterCollection parameters = command.Parameters;
            if (arguments != null)
            {
                int solt = 0;
                foreach (NSJSValue item in arguments)
                {
                    try
                    {
                        if (startIndex > solt)
                        {
                            continue;
                        }
                        IDbDataParameter parameter = ToDbDataParameter(adapter, item);
                        if (parameter == null)
                        {
                            continue;
                        }
                        parameters.Add(parameter);
                    }
                    finally
                    {
                        solt++;
                    }
                }
            }
            return command;
        }

        public static IDbDataParameter ToDbDataParameter(DatabaseAccessAdapter adapter, NSJSValue value)
        {
            if (adapter == null)
            {
                return null;
            }
            NSJSObject o = value as NSJSObject;
            if (o == null)
            {
                return null;
            }
            string name = (o.Get("Name") as NSJSString)?.Value;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            object rax = o.Get("Value").As<object>();
            return adapter.CreateParameter(name, rax);
        }

        public static void Call<TThis>(IntPtr info, Action<TThis, NSJSFunctionCallbackInfo> doo)
        {
            if (doo == null)
            {
                throw new ArgumentNullException("doo");
            }
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            TThis self = NSJSKeyValueCollection.Get<TThis>(arguments.This);
            if (self == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                try
                {
                    doo(self, arguments);
                }
                catch (Exception exception)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
            }
        }

        public static void Call<TThis>(IntPtr info, Func<TThis, NSJSFunctionCallbackInfo, NSJSValue, NSJSValue> doo)
        {
            if (doo == null)
            {
                throw new ArgumentNullException("doo");
            }
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            TThis self = NSJSKeyValueCollection.Get<TThis>(arguments.This);
            if (self == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                try
                {
                    NSJSValue solt0 = arguments.Length > 0 ? arguments[0] : null;
                    doo(self, arguments, solt0);
                }
                catch (Exception exception)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
            }
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, HttpFileCollection files)
        {
            if (machine == null)
            {
                return null;
            }
            if (files == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject owner = NSJSObject.New(machine);
            foreach (string key in files.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                HttpPostedFile fileinfo = files[key];
                if (fileinfo == null)
                {
                    continue;
                }
                owner.Set(key, ToObject(machine, fileinfo));
            }
            return owner;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, HttpPostedFile file)
        {
            if (machine == null)
            {
                return null;
            }
            if (file == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject objective = NSJSObject.New(machine);
            objective.Set("ContentLength", file.ContentLength);
            objective.Set("ContentType", file.ContentType);
            objective.Set("FileName", file.FileName);
            objective.Set("InputStream", NSJSValue.NullMerge(machine, NSJSStream.New(machine, file.InputStream)));
            return objective;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, IDbDataParameter parameter)
        {
            if (machine == null)
            {
                return null;
            }
            if (parameter == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject o = NSJSObject.New(machine);
            o.Set("Name", parameter.ParameterName);
            o.Set("IsNullable", parameter.IsNullable);
            o.Set("DbType", unchecked((int)parameter.DbType));
            return o;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, Cookie cookie)
        {
            if (machine == null)
            {
                return null;
            }
            if (cookie == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject objective = NSJSObject.New(machine);
            objective.Set("Comment", cookie.Comment);
            objective.Set("CommentUri", cookie.CommentUri?.ToString());
            objective.Set("Discard", cookie.Discard);
            objective.Set("Domain", cookie.Domain);
            objective.Set("Expired", cookie.Expired);
            objective.Set("Expires", cookie.Expires < NSJSDateTime.Min ? NSJSDateTime.Min : cookie.Expires);
            objective.Set("HttpOnly", cookie.HttpOnly);
            objective.Set("Name", cookie.Name);
            objective.Set("Path", cookie.Path);
            objective.Set("Port", cookie.Port);
            objective.Set("Secure", cookie.Secure);
            objective.Set("TimeStamp", cookie.TimeStamp);
            objective.Set("Value", cookie.Value);
            objective.Set("Version", cookie.Version);
            return objective;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, NameValueCollection s)
        {
            if (machine == null)
            {
                return null;
            }
            NSJSObject obj = NSJSObject.New(machine);
            if (s != null)
            {
                foreach (string key in s.AllKeys)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    string value = s.Get(key);
                    string name = key.ToLower();
                    if (value != null)
                    {
                        obj.Set(name, value);
                    }
                    else
                    {
                        obj.Set(name, NSJSValue.Null(machine));
                    }
                }
            }
            return obj;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, IDictionary<string, string> s)
        {
            if (machine == null)
            {
                return null;
            }
            NSJSObject obj = NSJSObject.New(machine);
            if (s != null)
            {
                foreach (KeyValuePair<string, string> kv in s)
                {
                    if (string.IsNullOrEmpty(kv.Key))
                    {
                        continue;
                    }
                    if (kv.Value == null)
                    {
                        obj.Set(kv.Key, NSJSValue.Null(machine));
                    }
                    else
                    {
                        obj.Set(kv.Key, kv.Value);
                    }
                }
            }
            return obj;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, KeyValuePair<string, string> kv)
        {
            if (machine == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(kv.Key))
            {
                return null;
            }
            NSJSObject obj = NSJSObject.New(machine);
            obj.Set("Key", kv.Key);
            if (kv.Value == null)
            {
                obj.Set("Value", NSJSValue.Null(machine));
            }
            else
            {
                obj.Set("Value", kv.Value);
            }
            return obj;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, EndPoint endpoint)
        {
            if (machine == null)
            {
                return null;
            }
            if (endpoint == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject obj = NSJSObject.New(machine);
            obj.Set("AddressFamily", (int)endpoint.AddressFamily);
            IPEndPoint ipep = endpoint as IPEndPoint;
            if (ipep != null)
            {
                obj.Set("Address", ToObject(machine, ipep.Address));
                obj.Set("Port", ipep.Port);
            }
            return obj;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, EncodingInfo encoding)
        {
            if (machine == null)
            {
                return null;
            }
            if (encoding == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject obj = NSJSObject.New(machine);
            obj.Set("Name", encoding.Name);
            obj.Set("DisplayName", encoding.DisplayName);
            obj.Set("CodePage", encoding.CodePage);
            return obj;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, IPHostEntry host)
        {
            if (machine == null)
            {
                return null;
            }
            if (host == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject entry = NSJSObject.New(machine);

            entry.Set("HostName", host.HostName);
            entry.Set("AddressList", ArrayAuxiliary.ToArray(machine, host.AddressList));

            string[] aliases = host.Aliases;
            NSJSArray array = NSJSArray.New(machine, aliases.Length);
            for (int i = 0; i < aliases.Length; i++)
            {
                array[i] = NSJSString.New(machine, aliases[i]);
            }
            entry.Set("Aliases", array);

            return entry;
        }

        public static IPAddress ToAddress(NSJSValue value)
        {
            if (value == null)
            {
                return null;
            }
            try
            {
                NSJSObject obj = value as NSJSObject;
                string address = null;
                if (obj != null)
                {
                    byte[] buffer = (obj.Get("AddressBytes") as NSJSUInt8Array)?.Buffer;
                    if (buffer != null)
                    {
                        return new IPAddress(buffer);
                    }
                    address = (obj.Get("Address") as NSJSString)?.Value;
                    if (!string.IsNullOrEmpty(address))
                    {
                        IPAddress addresst;
                        if (IPAddress.TryParse(address, out addresst))
                        {
                            return addresst;
                        }
                    }
                }
                address = (value as NSJSString)?.Value;
                if (!string.IsNullOrEmpty(address))
                {
                    IPAddress addresst;
                    if (IPAddress.TryParse(address, out addresst))
                    {
                        return addresst;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EndPoint ToEndPoint(NSJSValue value)
        {
            string ep = (value as NSJSString)?.Value;
            if (string.IsNullOrEmpty(ep))
            {
                return null;
            }
            try
            {
                return Ipep.ToIpep(ep);
            }
            catch
            {
                return null;
            }
        }

        public static int Fill(NSJSValue source, NameValueCollection destination)
        {
            NSJSObject objecttive = source as NSJSObject;
            int count = 0;
            if (objecttive == null || destination == null)
            {
                return count;
            }
            foreach (string key in objecttive.GetAllKeys())
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                destination.Set(key, ValueAuxiliary.ToString(objecttive.Get(key)));
                count++;
            }
            return count;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, object obj)
        {
            if (machine == null)
            {
                return null;
            }
            if (obj == null)
            {
                return NSJSValue.Null(machine);
            }
            Type owner = obj.GetType();
            NSJSObject objective = NSJSObject.New(machine);
            foreach (PropertyInfo pi in owner.GetProperties())
            {
                object value = pi.GetValue(obj, null);
                NSJSValue result = null;
                try
                {
                    do
                    {
                        if (value == null)
                        {
                            break;
                        }
                        Type clazz = pi.PropertyType;
                        Type element = TypeTool.GetArrayElement(clazz);
                        if (element == null && value is IList)
                        {
                            result = ArrayAuxiliary.ToArray(machine, element, (IList)value);
                        }
                        else if (TypeTool.IsBasicType(clazz) && !TypeTool.IsIPAddress(clazz))
                        {
                            result = value.As(machine);
                        }
                        else
                        {
                            result = ToObject(machine, value);
                        }
                    } while (false);
                }
                catch (Exception) { }
                if (result == null)
                {
                    result = NSJSValue.Null(machine);
                }
                objective.Set(pi.Name, result);
            }
            return objective;
        }

        public static int Fill(NSJSValue source, NSJSValue destination)
        {
            NSJSObject sourceObject = source as NSJSObject;
            NSJSObject destinationObject = destination as NSJSObject;
            int count = 0;
            if (sourceObject == null || destinationObject == null)
            {
                return count;
            }
            foreach (string key in sourceObject.GetAllKeys())
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                count++;
                NSJSValue value = sourceObject.Get(key);
                destinationObject.Set(key, value);
            }
            return count;
        }

        public static NSJSValue ToObject(NSJSVirtualMachine machine, IPAddress address)
        {
            if (machine == null)
            {
                return null;
            }
            if (address == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject o = NSJSObject.New(machine);
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                o.Set("ScopeId", address.ScopeId);
            }
            o.Set("AddressFamily", (int)address.AddressFamily);
            o.Set("Address", address.ToString());
            o.Set("AddressBytes", address.GetAddressBytes());
            return o;
        }
    }
}
