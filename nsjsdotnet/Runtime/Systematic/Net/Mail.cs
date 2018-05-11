namespace nsjsdotnet.Runtime.Systematic.Net
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.Net;
    using System;
    using System.Net.Mail;

    sealed class Mail
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private static readonly NSJSFunctionCallback m_SendAsyncProc;
        private static readonly NSJSFunctionCallback g_SendProc;
        private static readonly NSJSFunctionCallback g_CloseProc;

        static Mail()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.AddFunction("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(New));
            g_SendProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Send);
            g_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
            m_SendAsyncProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SendAsync);
        }

        private static void Close(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(ObjectAuxiliary.RemoveInKeyValueCollection(arguments.This));
        }

        private static void Send(IntPtr info)
        {
            InternalSend(info, true);
        }

        private static void SendAsync(IntPtr info)
        {
            InternalSend(info, false);
        }

        private static void InternalSend(IntPtr info, bool synchronization)
        {
            ObjectAuxiliary.Call<MailClient>(info, (smtp, arguments) =>
            {
                do
                {
                    NSJSVirtualMachine machine = arguments.VirtualMachine;
                    if (arguments.Length <= 0)
                    {
                        Throwable.ArgumentException(machine);
                        break;
                    }
                    MailMessage message = null;
                    try
                    {
                        message = ObjectAuxiliary.ToMailMessage(arguments[0]);
                    }
                    catch (Exception exception)
                    {
                        Throwable.Exception(machine, exception);
                        break;
                    }
                    if (message == null)
                    {
                        Throwable.ArgumentNullException(machine);
                        break;
                    }
                    if (synchronization)
                    {
                        arguments.SetReturnValue(smtp.Send(message));
                    }
                    else
                    {
                        NSJSFunction function = arguments.Length > 1 ? arguments[1] as NSJSFunction : null;
                        Action<Exception> callbackt = null;
                        if (function != null)
                        {
                            callbackt = (exception) => machine.Join((sender, state) =>
                                function.Call(new[] { Throwable.FormatMessage(exception) }));
                            function.CrossThreading = true;
                        }
                        arguments.SetReturnValue(smtp.SendAsync(message, callbackt));
                    }
                } while (false);
            });
        }

        public static NSJSObject New(NSJSVirtualMachine machine, MailClient smtp)
        {
            if (machine == null || smtp == null)
            {
                return null;
            }
            NSJSObject o = NSJSObject.New(machine);
            o.Set("Send", g_SendProc);
            o.Set("SendAsync", m_SendAsyncProc);
            o.Set("Close", g_CloseProc);
            o.Set("Dispose", g_CloseProc);
            NSJSKeyValueCollection.Set(o, smtp);
            return o;
        }

        public static void New(NSJSFunctionCallbackInfo arguments)
        {
            NSJSObject options = arguments.Length > 0 ? arguments[0] as NSJSObject : null;
            if (options == null)
            {
                Throwable.ArgumentNullException(arguments.VirtualMachine);
            }
            else
            {
                string username = options.Get("UserName").As<string>();
                string password = options.Get("Password").As<string>();
                string domain = options.Get("Domain").As<string>();
                string server = options.Get("Server").As<string>();
                if (username == null || password == null || server == null)
                {
                    Throwable.ArgumentNullException(arguments.VirtualMachine);
                }
                else if (username.Length <= 0 || password.Length <= 0 || server.Length <= 0)
                {
                    Throwable.ArgumentException(arguments.VirtualMachine);
                }
                else
                {
                    int port = MailClient.DefaultPort;
                    NSJSInt32 i = (options.Get("Port") as NSJSInt32);
                    if (i != null)
                    {
                        port = i.Value;
                    }
                    MailClient smtp = new MailClient(server, port, username, password, domain);
                    smtp.EnableSsl = options.Get("EnableSsl").As<bool>();
                    i = (options.Get("Timeout") as NSJSInt32);
                    if (i != null)
                    {
                        smtp.Timeout = i.Value;
                    }
                    arguments.SetReturnValue(New(arguments.VirtualMachine, smtp));
                }
            }
        }
    }
}
