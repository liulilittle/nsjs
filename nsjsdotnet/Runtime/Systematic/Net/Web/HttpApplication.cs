namespace nsjsdotnet.Runtime.Systematic.Net.Web
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.Linq;
    using nsjsdotnet.Core.Net.Web;
    using System;
    using System.Collections.Generic;
    using System.Threading;
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

        private static readonly EventHandler<HttpBeginProcessRequestEventArgs> g_BeginProcessRequestProc;
        private static readonly EventHandler<HTTPContext> g_EndProcessRequestProc;

        static HttpApplication()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            g_StartProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Start);
            g_StopProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Stop);
            g_RootProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Root);
            g_NameProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Name);
            g_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
            owner.Set("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));
            owner.Set("Invalid", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Invalid));
            g_EndProcessRequestProc = OnEndProcessRequest;
            g_BeginProcessRequestProc = OnBeginProcessRequest;
        }

        private class HttpHandler : IHTTPHandler
        {
            private static readonly NSJSFunctionCallback m_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
            private static readonly NSJSFunctionCallback m_AsynchronousProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Asynchronous);

            private static void Close(IntPtr info)
            {
                NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
                NSJSObject self = arguments.This;
                ObjectAuxiliary.RemoveInKeyValueCollection(self);
                HttpRequest.Close(self.Get("Request"));
                HttpResponse.Close(self.Get("Response") as NSJSObject);
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
                NSJSObject currentHandler = this.GetCurrentHandler();
                if (currentHandler == null)
                {
                    return null;
                }
                return currentHandler.Get("ProcessRequest") as NSJSFunction;
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
                if (context == null)
                {
                    return /*undefined*/;
                }
                NSJSVirtualMachine machine = this.GetVirtualMachine();
                machine.Join((sender, state) =>
                {
                    NSJSFunction function = this.GetProcessRequestCallback();
                    if (function != null)
                    {
                        NSJSObject context_object = null;
                        try
                        {
                            context_object = this.NewContextObject(context);
                        }
                        catch (Exception) { /*-----*/ }
                        if (context_object != null)
                        {
                            function.Call(context_object);
                        }
                    }
                });
            }

            public NSJSObject NewContextObject(HTTPContext context)
            {
                return NewContextObject(this.GetVirtualMachine(), this.Source, context);
            }

            public static NSJSObject NewContextObject(NSJSVirtualMachine machine,
                NSJSObject application,
                HTTPContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
                if (application == null)
                {
                    throw new ArgumentNullException("application");
                }
                if (machine == null)
                {
                    throw new ArgumentNullException("machine");
                }
                NSJSObject objective = NSJSObject.New(machine); // ctx
                objective.Set("Application", application);
                objective.Set("Close", m_CloseProc);
                objective.Set("Dispose", m_CloseProc);
                objective.Set("Request", HttpRequest.New(machine, objective, context.Request));
                objective.Set("Response", HttpResponse.New(machine, objective, context.Response));
                objective.DefineProperty("Asynchronous", m_AsynchronousProc, m_AsynchronousProc);
                NSJSKeyValueCollection.Set(objective, context);
                return objective;
            }

            private static void Asynchronous(IntPtr info)
            {
                ObjectAuxiliary.GetOrSetProperty<HTTPContext>(info, (context) => context.Asynchronous, (context, value) => context.Asynchronous = value);
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
                catch (Exception) { /*---------------------------------------------------------*/ }
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
                arguments.SetReturnValue(application.Name);
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
                arguments.SetReturnValue(application.Root);
            }
        }

        public static NSJSObject New(NSJSVirtualMachine machine, HTTPApplication application)
        {
            if (machine == null || application == null)
            {
                return null;
            }
            NSJSObject objective = NSJSObject.New(machine);
            objective.DefineProperty("Name", g_NameProc, g_NameProc);
            objective.DefineProperty("Root", g_RootProc, g_RootProc);
            objective.Set("Start", g_StartProc);
            objective.Set("Stop", g_StopProc);
            objective.Set("Close", g_CloseProc);
            objective.Set("Dispose", g_CloseProc);

            application.Tag = objective;
            objective.CrossThreading = true;

            application.Handler = new HttpHandler(objective, application);
            application.EndProcessRequest += g_EndProcessRequestProc;
            application.BeginProcessRequest += g_BeginProcessRequestProc;

            NSJSKeyValueCollection.Set(objective, application);
            return objective;
        }

        private static void OnEndProcessRequest(object sender, HTTPContext e)
        {
            DoProcessRequest(sender, (application, origin, machine) =>
            {
                NSJSFunction callback = origin.Get("EndProcessRequest") as NSJSFunction;
                if (callback != null)
                {
                    callback.Call(HttpHandler.NewContextObject(machine, origin, e));
                }
            });
        }

        private static void DoProcessRequest(object sender, Action<HTTPApplication, NSJSObject, NSJSVirtualMachine> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            HTTPApplication application = sender as HTTPApplication;
            if (application == null)
            {
                return /* false */;
            }
            NSJSObject origin = application.Tag as NSJSObject;
            if (origin == null)
            {
                return /* false */;
            }
            NSJSVirtualMachine machine = origin.VirtualMachine;
            machine.Join((M, X) => callback(application, origin, machine));
        }

        private static void OnBeginProcessRequest(object sender, HttpBeginProcessRequestEventArgs e)
        {
            DoProcessRequest(sender, (application, origin, machine) =>
            {
                NSJSFunction callback = origin.Get("BeginProcessRequest") as NSJSFunction;
                if (callback != null)
                {
                    NSJSObject args = NSJSObject.New(machine);
                    args.Set("Cancel", e.Cancel);
                    args.Set("Application", origin);
                    args.Set("CurrentContext", HttpHandler.NewContextObject(machine, origin, e.CurrentContext));
                    callback.Call(args);
                    e.Cancel = args.Get("Cancel").As<bool>();
                }
            });
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(New(arguments.VirtualMachine, new HTTPApplication()));
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
