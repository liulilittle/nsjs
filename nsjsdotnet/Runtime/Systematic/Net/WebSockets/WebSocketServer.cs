namespace nsjsdotnet.Runtime.Systematic.Net.WebSockets
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Concurrent;
    using WebSocketListener = nsjsdotnet.Core.Net.WebSocket.WebSocketServer;
    using nsjsdotnet.Core.Net.WebSocket;

    static class WebSocketServer
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private static readonly NSJSFunctionCallback g_GetBindPortProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetBindPort);
        private static readonly NSJSFunctionCallback g_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
        private static readonly NSJSFunctionCallback g_StopProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Stop);
        private static readonly NSJSFunctionCallback g_StartProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Start);

        static WebSocketServer()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.AddFunction("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));
        }

        private static void GetBindPort(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocketListener>(info, (server, arguments) => arguments.SetReturnValue(server.GetBindPort()));
        }

        private static NSJSObject New(NSJSVirtualMachine machine, WebSocketListener server)
        {
            if (machine == null || server == null)
            {
                return null;
            }
            NSJSObject objective = NSJSObject.New(machine);

            objective.Set("Port", server.Port);
            objective.Set("GetBindPort", g_GetBindPortProc);
            objective.Set("Stop", g_StopProc);
            objective.Set("Start", g_StartProc);
            objective.Set("Close", g_CloseProc);
            objective.Set("Dispose", g_CloseProc);

            server.UserToken = objective;
            objective.CrossThreading = true;

            server.OnMessage += (websocket, e) => ProcessEvent(objective, websocket, "OnMessage", e);
            server.OnOpen += (websocket, e) => ProcessEvent(objective, websocket, "OnOpen", e, true);
            server.OnError += (websocket, e) => ProcessEvent(objective, websocket, "OnError", e);
            server.OnClose += (websocket, e) => ProcessEvent(objective, websocket, "OnClose", e);

            NSJSKeyValueCollection.Set(objective, server);
            return objective;
        }

        private static bool ProcessEvent(NSJSObject server, WebSocket socket, string evt, EventArgs e, bool newing = false)
        {
            if (server == null || socket == null || string.IsNullOrEmpty(evt))
            {
                return false;
            }
            NSJSVirtualMachine machine = server.VirtualMachine;
            machine.Join(delegate
            {
                NSJSObject socketclient = WebSocketClient.Get(socket);
                if (newing && socketclient == null)
                {
                    socketclient = WebSocketClient.New(machine, socket);
                }
                NSJSFunction function = server.Get(evt) as NSJSFunction;
                if (function != null)
                {
                    NSJSObject data = WebSocketClient.GetMessageEventData(machine, e);
                    function.Call(new NSJSValue[] { socketclient, data });
                }
            });
            return true;
        }

        private static void Start(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocketListener>(info, (server, arguments) =>
            {
                try
                {
                    server.Start();
                }
                catch (Exception exception)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
            });
        }

        private static void Stop(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocketListener>(info, (server, arguments) => server.Stop());
        }

        private static void Close(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocketListener>(info, (server, arguments) =>
            {
                server.Stop();
                ObjectAuxiliary.RemoveInKeyValueCollection(arguments.This);
            });
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            int port = (arguments.Length > 0 ? (arguments[0] as NSJSInt32)?.Value : null).GetValueOrDefault();
            arguments.SetReturnValue(New(arguments.VirtualMachine, new WebSocketListener(port)));
        }
    }
}
