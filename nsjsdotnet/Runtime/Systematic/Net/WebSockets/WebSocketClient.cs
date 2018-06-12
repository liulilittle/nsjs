namespace nsjsdotnet.Runtime.Systematic.Net.WebSockets
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.Net.WebSocket;
    using System;

    static class WebSocketClient
    {
        private static readonly NSJSFunctionCallback m_OpenProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Open);
        private static readonly NSJSFunctionCallback m_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
        private static readonly NSJSFunctionCallback m_SendProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Send);
        private static readonly NSJSFunctionCallback m_AvailableProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Available);
        private static readonly NSJSFunctionCallback m_NoDelayProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(NoDelay);

        private static readonly EventHandler<MessageEventArgs> m_OnMessageProc = OnMessage;
        private static readonly EventHandler<EventArgs> m_OnOpenProc = OnOpen;
        private static readonly EventHandler<ErrorEventArgs> m_OnErrorProc = OnError;
        private static readonly EventHandler<CloseEventArgs> m_OnCloseProc = OnClose;

        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static WebSocketClient()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.AddFunction("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));
        }

        public static NSJSObject New(NSJSVirtualMachine machine, WebSocket websocket)
        {
            if (machine == null || websocket == null)
            {
                return null;
            }
            object usertoken = websocket.UserToken;
            if (usertoken != null)
            {
                return usertoken as NSJSObject;
            }
            NSJSObject objective = NSJSObject.New(machine);
            objective.Set("Open", m_OpenProc);
            objective.Set("Close", m_CloseProc);
            objective.Set("Dispose", m_CloseProc);
            objective.Set("Send", m_SendProc);
            objective.Set("Path", websocket.Path);

            objective.Set("OnMessage", NSJSValue.Null(machine));
            objective.Set("OnClose", NSJSValue.Null(machine));
            objective.Set("OnError", NSJSValue.Null(machine));
            objective.Set("OnOpen", NSJSValue.Null(machine));

            websocket.OnMessage += m_OnMessageProc;
            websocket.OnOpen += m_OnOpenProc;
            websocket.OnError += m_OnErrorProc;
            websocket.OnClose += m_OnCloseProc;

            websocket.UserToken = objective;
            objective.CrossThreading = true;

            objective.DefineProperty("NoDelay", m_NoDelayProc, m_NoDelayProc);
            objective.DefineProperty("Available", m_AvailableProc, (NSJSFunctionCallback)null);
            return objective;
        }

        public static NSJSObject GetMessageEventData(NSJSVirtualMachine machine, EventArgs e)
        {
            if (machine == null || e == null)
            {
                return null;
            }
            MessageEventArgs message = e as MessageEventArgs;
            if (message == null)
            {
                return null;
            }
            NSJSObject data = NSJSObject.New(machine);
            data.Set("IsText", message.IsText);
            if (message.IsText)
            {
                data.Set("Message", message.Message);
            }
            else // BLOB
            {
                data.Set("RawData", message.RawData);
            }
            return data;
        }

        public static NSJSObject Get(WebSocket socket)
        {
            if (socket == null)
            {
                return null;
            }
            return socket.UserToken as NSJSObject;
        }

        private static bool ProcessEvent(object sender, string evt, EventArgs e)
        {
            if (sender == null || string.IsNullOrEmpty(evt))
            {
                return false;
            }
            WebSocket socket = sender as WebSocket;
            if (socket == null)
            {
                return false;
            }
            NSJSObject websocket = Get(socket);
            if (websocket == null)
            {
                return false;
            }
            NSJSVirtualMachine machine = websocket.VirtualMachine;
            machine.Join(delegate
            {
                NSJSObject data = WebSocketClient.GetMessageEventData(machine, e);
                data.Set("Socket", websocket);
                ObjectAuxiliary.SendEvent(websocket, evt, data);
            });
            return true;
        }

        private static void OnMessage(object sender, MessageEventArgs e)
        {
            ProcessEvent(sender, "OnMessage", e);
        }

        private static void OnOpen(object sender, EventArgs e)
        {
            ProcessEvent(sender, "OnOpen", e);
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            ProcessEvent(sender, "OnError", e);
        }

        private static void OnClose(object sender, CloseEventArgs e)
        {
            ProcessEvent(sender, "OnClose", e);
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string url = arguments.Length > 0 ? (arguments[0] as NSJSString)?.Value : null;
            NSJSVirtualMachine machine = arguments.VirtualMachine;
            if (url == null)
            {
                Throwable.ArgumentNullException(machine);
            }
            else if ((url = url.Trim()).Length <= 0)
            {
                Throwable.ArgumentException(machine);
            }
            else
            {
                try
                {
                    arguments.SetReturnValue(New(machine, new WebSocket(url)));
                }
                catch (Exception exception)
                {
                    Throwable.Exception(machine, exception);
                }
            }
        }

        private static void NoDelay(IntPtr info)
        {
            ObjectAuxiliary.GetOrSetProperty<WebSocket>(info, (websocket) => websocket.NoDelay, (websocket, value) => websocket.NoDelay = value);
        }

        private static void Available(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocket>(info, (websocket, arguments) => arguments.SetReturnValue(websocket.Available));
        }

        private static void Close(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocket>(info, (websocket, arguments) =>
            {
                websocket.Close();
                arguments.SetReturnValue(ObjectAuxiliary.RemoveInKeyValueCollection(arguments.This));
            });
        }

        private static void Send(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocket>(info, (websocket, arguments) =>
            {
                bool success = false;
                byte[] buffer = arguments.Length > 0 ? (arguments[0] as NSJSUInt8Array)?.Buffer : null;
                if (buffer != null)
                {
                    success = websocket.Send(buffer);
                }
                else
                {
                    string message = arguments.Length > 0 ? (arguments[1] as NSJSString)?.Value : null;
                    if (message != null)
                    {
                        success = websocket.Send(message);
                    }
                }
                arguments.SetReturnValue(success);
            });
        }

        private static void Open(IntPtr info)
        {
            ObjectAuxiliary.Call<WebSocket>(info, (websocket, arguments) =>
            {
                try
                {
                    websocket.Open();
                }
                catch (Exception exception)
                {
                    Throwable.Exception(arguments.VirtualMachine, exception);
                }
            });
        }
    }
}
