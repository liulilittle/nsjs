namespace nsjsdotnet.Runtime.Systematic.Net.Web
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using HTTPApplication = nsjsdotnet.Core.Net.Web.HttpApplication;
    using HTTPContext = nsjsdotnet.Core.Net.Web.HttpContext;
    using IHTTPHandler = nsjsdotnet.Core.Net.Web.IHttpHandler;

    static class HttpApplication
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private static readonly NSJSFunctionCallback g_StartProc;
        private static readonly NSJSFunctionCallback g_StopProc;
        private static readonly NSJSFunctionCallback g_RootProc;
        private static readonly NSJSFunctionCallback g_NameProc;
        private static readonly NSJSFunctionCallback g_CloseProc;

        static HttpApplication()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            g_StartProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Start);
            g_StopProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Stop);
            g_RootProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Root);
            g_NameProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Name);
            g_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
            owner.AddFunction("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));
            owner.AddFunction("Invalid", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Invalid));
        }

        private class HttpHandler : IHTTPHandler
        {
            private static readonly NSJSFunctionCallback m_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
            private NSJSFunction m_ProcessRequestCallback = null;

            private static void Close(IntPtr info)
            {
                NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
                NSJSObject self = arguments.This;
                HttpRequest.Close(self.Get("Request"));
                HttpResponse.Close(self.Get("Response"));
            }

            public HTTPApplication Application
            {
                get;
                private set;
            }

            public NSJSObject Source
            {
                get;
                private set;
            }

            protected virtual NSJSVirtualMachine GetVirtualMachine()
            {
                return this.Source.VirtualMachine;
            }

            protected virtual NSJSObject GetCurrentHandler()
            {
                return this.Source.Get("Handler") as NSJSObject;
            }

            protected virtual NSJSFunction GetProcessRequestCallback()
            {
                lock (this)
                {
                    if (this.m_ProcessRequestCallback == null)
                    {
                        NSJSObject currentHandler = this.GetCurrentHandler();
                        if (currentHandler != null)
                        {
                            this.m_ProcessRequestCallback = currentHandler.Get("ProcessRequest") as NSJSFunction;
                            this.m_ProcessRequestCallback.CrossThreading = true;
                        }
                    }
                    return this.m_ProcessRequestCallback;
                }
            }

            public HttpHandler(NSJSObject source, HTTPApplication application)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
                if (application == null)
                {
                    throw new ArgumentNullException("application");
                }
                source.CrossThreading = true;
                this.Source = source;
                this.Application = application;
            }

            public void ProcessRequest(HTTPContext context)
            {
                this.OnProcessRequest(context);
            }

            public NSJSObject NewContextObject(HTTPContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
                NSJSVirtualMachine machine = this.GetVirtualMachine();
                NSJSObject objective = NSJSObject.New(machine); // ctx
                objective.Set("Application", this.Source);
                objective.Set("Close", m_CloseProc);
                objective.Set("Dispose", m_CloseProc);
                objective.Set("Request", HttpRequest.New(machine, objective, context.Request));
                objective.Set("Response", HttpResponse.New(machine, objective, context.Response));
                return objective;
            }

            protected virtual void OnProcessRequest(HTTPContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
                NSJSVirtualMachine machine = this.GetVirtualMachine();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                machine.Join((sender, state) =>
                {
                    NSJSFunction function = this.GetProcessRequestCallback();
                    if (function != null)
                    {
                        NSJSObject context_object = this.NewContextObject(context);
                        function.Call(context_object);
                    }
                });
                stopwatch.Stop();
            }
        }

        private static void Close(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            HTTPApplication application;
            NSJSKeyValueCollection.Release(arguments.This, out application);
            if (application == null)
            {
                arguments.SetReturnValue(false);
            }
            else
            {
                bool success = false;
                try
                {
                    application.Stop();
                    success = true;
                }
                catch (Exception) { }
                arguments.SetReturnValue(success);
            }
        }

        private static void Name(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            HTTPApplication application = GetApplication(arguments.This);
            if (application == null)
            {
                arguments.SetReturnValue(NSJSValue.Undefined(arguments.VirtualMachine));
            }
            else if (arguments.Length > 0)
            {
                string name = (arguments[0] as NSJSString)?.Value;
                application.Name = name;
            }
            else
            {
                arguments.SetReturnValue(application.Name ?? string.Empty);
            }
        }

        private static void Invalid(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(GetApplication(arguments.This) == null);
        }

        public static HTTPApplication GetApplication(NSJSObject application)
        {
            if (application == null)
            {
                return null;
            }
            return NSJSKeyValueCollection.Get<HTTPApplication>(application);
        }

        private static void Root(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            HTTPApplication application = GetApplication(arguments.This);
            if (application == null)
            {
                arguments.SetReturnValue(NSJSValue.Undefined(arguments.VirtualMachine));
            }
            else if (arguments.Length > 0)
            {
                string path = (arguments[0] as NSJSString)?.Value;
                application.Root = path;
            }
            else
            {
                arguments.SetReturnValue(application.Root ?? string.Empty);
            }
        }

        public static NSJSObject New(NSJSVirtualMachine machine, HTTPApplication application)
        {
            if (machine == null || application == null)
            {
                return null;
            }
            NSJSObject o = new NSJSObject(machine);
            o.Set("Name", g_NameProc);
            o.Set("Handler", NSJSValue.Null(machine));
            o.Set("Root", g_RootProc);
            o.Set("Start", g_StartProc);
            o.Set("Stop", g_StopProc);
            o.Set("Close", g_CloseProc);
            o.Set("Dispose", g_CloseProc);
            application.Handler = new HttpHandler(o, application);
            NSJSKeyValueCollection.Set(o, application);
            return o;
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(HttpApplication.New(arguments.VirtualMachine, new HTTPApplication()));
        }

        private static void Start(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            HTTPApplication application = GetApplication(arguments.This);
            if (application == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                try
                {
                    IList<string> prefixes = ArrayAuxiliary.ToStringList(arguments.Length > 0 ? arguments[0] : null);
                    do
                    {
                        if (prefixes.IsNullOrEmpty())
                        {
                            Throwable.ArgumentNullException(arguments.VirtualMachine);
                            break;
                        }
                        application.Start(prefixes);
                        arguments.SetReturnValue(true);
                    } while (false);
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            }
        }

        private static void Stop(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            HTTPApplication application = GetApplication(arguments.This);
            if (application == null)
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                application.Stop();
                arguments.SetReturnValue(true);
            }
        }
    }
}
