namespace nsjsdotnet.Runtime.Systematic.Net.Sockets
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.Net;
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using SOCKET = System.Net.Sockets.Socket;

    sealed class Socket // Mrs. Potato Head
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private static readonly NSJSFunctionCallback m_SendProc;
        private static readonly NSJSFunctionCallback m_CloseProc;
        private static readonly NSJSFunctionCallback m_BindProc;
        private static readonly NSJSFunctionCallback m_ConnectProc;
        private static readonly NSJSFunctionCallback m_ReceiveProc;
        private static readonly NSJSFunctionCallback m_AcceptProc;
        private static readonly NSJSFunctionCallback m_ConnectedProc;
        private static readonly NSJSFunctionCallback m_ConnectAsyncProc;
        private static readonly NSJSFunctionCallback m_AcceptAsyncProc;
        private static readonly NSJSFunctionCallback m_SendAsyncProc;
        private static readonly NSJSFunctionCallback m_ReceiveAsyncProc;
        private static readonly NSJSFunctionCallback m_SendToProc;
        private static readonly NSJSFunctionCallback m_SendToAsyncProc;
        private static readonly NSJSFunctionCallback m_ReceiveFromProc;
        private static readonly NSJSFunctionCallback m_ReceiveFromAsyncProc;

        private class SocketContext
        {
            public SocketAsyncEventArgs ConnectedAsync = null;
            public SocketAsyncEventArgs AcceptAsync = null;
            public SOCKET Socket = null;
            public NSJSFunction AcceptAsyncCallback = null;
            public NSJSFunction ConnectedAsyncCallback = null;
            public NSJSObject This = null;
        }

        static Socket()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(New));
            owner.Set("Invalid", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Invalid));
            owner.Set("GetActiveTcpListeners", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetActiveTcpListeners));
            owner.Set("GetActiveTcpListeners", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetActiveUdpListeners));
            m_SendProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Send);
            m_BindProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Bind);
            m_CloseProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Close);
            m_ConnectProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Connect);
            m_ReceiveProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Receive);
            m_AcceptProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Accept);
            m_ConnectedProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Connected);
            m_ConnectAsyncProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ConnectAsync);
            m_AcceptAsyncProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(AcceptAsync);
            m_SendAsyncProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SendAsync);
            m_ReceiveAsyncProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReceiveAsync);
            m_SendToProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SendTo);
            m_SendToAsyncProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(SendToAsync);
            m_ReceiveFromProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReceiveFrom);
            m_ReceiveFromAsyncProc = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ReceiveFromAsync);
        }

        private static void Invalid(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            SocketContext context = GetSocketContext(arguments.Length > 0 ? arguments[0] as NSJSObject : null);
            NSJSObject o = NSJSObject.New(arguments.VirtualMachine);
            arguments.SetReturnValue(!(context != null && context.Socket != null && context.This != null));
        }

        private static void GetActiveTcpListeners(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(ArrayAuxiliary.ToArray(arguments.VirtualMachine, IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()));
        }

        private static void GetActiveUdpListeners(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(ArrayAuxiliary.ToArray(arguments.VirtualMachine, IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()));
        }

        private static NSJSObject New(NSJSVirtualMachine machine, SOCKET socket)
        {
            if (machine == null || socket == null)
            {
                return null;
            }
            NSJSObject o = NSJSObject.New(machine);
            o.Set("MSS", SocketExtension.MSS);
            o.Set("PPP", SocketExtension.PPP);
            o.Set("MTU", SocketExtension.MTU);
            o.Set("LocalEndPoint", ObjectAuxiliary.ToObject(machine, socket.LocalEndPoint));
            o.Set("RemoteEndPoint", ObjectAuxiliary.ToObject(machine, socket.RemoteEndPoint));
            o.Set("Send", m_SendProc);
            o.Set("Bind", m_BindProc);
            o.Set("Handle", socket.Handle.ToInt32());
            o.Set("Close", m_CloseProc);
            o.Set("Dispose", m_CloseProc);
            o.Set("Connect", m_ConnectProc);
            o.Set("Receive", m_ReceiveProc);
            o.Set("Accept", m_AcceptProc);
            o.Set("Connected", m_ConnectedProc);
            o.Set("ConnectAsync", m_ConnectAsyncProc);
            o.Set("AcceptAsync", m_AcceptAsyncProc);
            o.Set("SendAsync", m_SendAsyncProc);
            o.Set("ReceiveAsync", m_ReceiveAsyncProc);
            o.Set("SendTo", m_SendToProc);
            o.Set("SendToAsync", m_SendToAsyncProc);
            o.Set("ReceiveFrom", m_ReceiveFromProc);
            o.Set("ReceiveFromAsync", m_ReceiveFromAsyncProc);
            o.CrossThreading = true;
            NSJSKeyValueCollection.Set(o, new SocketContext
            {
                This = o,
                Socket = socket,
            });
            return o;
        }

        private static void AcceptAsync(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            do
            {
                SocketContext context = GetSocketContext(arguments.This);
                if (context == null)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                SOCKET socket = context.Socket;
                if (socket == null || SocketExtension.CleanedUp(socket))
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                NSJSFunction callback = arguments.Length > 0 ? arguments[0] as NSJSFunction : null;
                try
                {
                    SocketAsyncEventArgs e = context.AcceptAsync;
                    if (e != null)
                    {
                        break;
                    }
                    e = new SocketAsyncEventArgs();
                    e.Completed += ProcessAccept;
                    e.UserToken = context;
                    context.AcceptAsync = e;
                    if (callback != null)
                    {
                        NSJSObject socketobject = arguments.This;
                        callback.CrossThreading = true;
                        context.AcceptAsyncCallback = callback;
                    }
                    if (!socket.AcceptAsync(e))
                    {
                        ProcessAccept(socket, e);
                    }
                    success = true;
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            } while (false);
            arguments.SetReturnValue(success);
        }

        private static void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                SocketContext context = e.UserToken as SocketContext;
                do
                {
                    SOCKET client = e.AcceptSocket;
                    SOCKET server = (SOCKET)sender;
                    e.AcceptSocket = null;
                    do
                    {
                        if (context == null)
                        {
                            break;
                        }
                        NSJSFunction function = context.AcceptAsyncCallback;
                        NSJSObject socket = context.This;
                        if (function == null)
                        {
                            break;
                        }
                        NSJSVirtualMachine machine = function.VirtualMachine;
                        if (machine == null)
                        {
                            break;
                        }
                        machine.Join((sendert, statet) => function.Call(socket,
                            NSJSInt32.New(machine, unchecked((int)e.SocketError)),
                            NSJSValue.NullMerge(machine, New(machine, client))));
                    } while (false);
                    if (!server.AcceptAsync(e))
                    {
                        ProcessAccept(server, e);
                    }
                } while (false);
            }
            catch (Exception) { }
        }

        private static void ConnectAsync(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            do
            {
                if (arguments.Length <= 0)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                SocketContext context = GetSocketContext(arguments.This);
                if (context == null)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                SOCKET socket = context.Socket;
                if (socket == null || SocketExtension.CleanedUp(socket))
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                EndPoint remoteEP = ObjectAuxiliary.ToEndPoint(arguments[0]);
                int cbsolt = 1;
                if (remoteEP == null)
                {
                    IPAddress address = ObjectAuxiliary.ToAddress(arguments[0]);
                    if (address == null)
                    {
                        break;
                    }
                    int port = arguments.Length > 1 ? ((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault() : 0;
                    remoteEP = new IPEndPoint(address, port);
                    cbsolt++;
                }
                if (remoteEP == null)
                {
                    break;
                }
                NSJSFunction callback = arguments.Length > cbsolt ? arguments[cbsolt] as NSJSFunction : null;
                try
                {
                    SocketAsyncEventArgs e = context.ConnectedAsync;
                    if (e != null)
                    {
                        break;
                    }
                    else
                    {
                        e = new SocketAsyncEventArgs();
                        e.Completed += ProcessConnected;
                        e.UserToken = context;
                        context.ConnectedAsync = e;
                    }
                    e.RemoteEndPoint = remoteEP;
                    if (callback != null)
                    {
                        callback.CrossThreading = true;
                        context.ConnectedAsyncCallback = callback;
                    }
                    if (!socket.ConnectAsync(e))
                    {
                        ProcessConnected(socket, e);
                    }
                    success = true;
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            } while (false);
            arguments.SetReturnValue(success);
        }

        private static void ProcessConnected(object sender, SocketAsyncEventArgs e)
        {
            using (e)
            {
                try
                {
                    SocketContext context = e.UserToken as SocketContext;
                    do
                    {
                        if (context == null)
                        {
                            break;
                        }
                        NSJSFunction function = context.ConnectedAsyncCallback;
                        NSJSObject socket = context.This;
                        context.ConnectedAsync = null;
                        context.ConnectedAsyncCallback = null;
                        if (function == null)
                        {
                            break;
                        }
                        NSJSVirtualMachine machine = function.VirtualMachine;
                        if (machine == null)
                        {
                            break;
                        }
                        machine.Join((sendert, statet) => function.Call(socket, NSJSInt32.New(machine, unchecked((int)e.SocketError))));
                    } while (false);
                }
                catch (Exception) { }
            }
        }

        private static void Connected(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            SOCKET socket = GetSocket(arguments.This);
            if (socket == null || SocketExtension.CleanedUp(socket))
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                arguments.SetReturnValue(socket.Connected);
            }
        }

        private static void Accept(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            SOCKET socket = GetSocket(arguments.This);
            if (socket == null || SocketExtension.CleanedUp(socket))
            {
                Throwable.ObjectDisposedException(arguments.VirtualMachine);
            }
            else
            {
                try
                {
                    arguments.SetReturnValue(New(arguments.VirtualMachine, socket.Accept()));
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            }
        }

        private static void Receive(IntPtr info)
        {
            InternalReceive(info, false, (callmode, socket, data, buffer, ofs, count, flags, remoteep) =>
            {
                int len = 0;
                if (callmode == 0)
                {
                    SocketExtension.Receive(socket, buffer, 0, count, flags, out len);
                }
                else if (callmode == 1)
                {
                    if (SocketExtension.Receive(socket, buffer, ofs, count, flags, out len))
                    {
                        for (int i = ofs; i < len; i++)
                        {
                            data[i] = buffer[i];
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException("callmode");
                }
                return len;
            });
        }

        private static void ReceiveFrom(IntPtr info)
        {
            InternalReceive(info, true, (callmode, socket, data, buffer, ofs, count, flags, remoteep) =>
            {
                EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                int len = 0;
                bool success = false;
                if (callmode == 0)
                {
                    success = SocketExtension.ReceiveForm(socket, buffer, 0, count, flags, ref endpoint, out len);
                }
                else if (callmode == 1)
                {
                    if ((success = SocketExtension.ReceiveForm(socket, buffer, ofs, count, flags, ref endpoint, out len)))
                    {
                        for (int i = ofs; i < len; i++)
                        {
                            data[i] = buffer[i];
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException("callmode");
                }
                if (success)
                {
                    ObjectAuxiliary.Fill(ObjectAuxiliary.ToObject(remoteep.VirtualMachine, endpoint), remoteep);
                }
                return len;
            });
        }

        private static void ReceiveAsync(IntPtr info)
        {
            InternalReceiveAsync(info, false, (socket, socketobject, data, buffer, ofs, count, flags, remoteep, callback) =>
            {
                bool success = false;
                if ((success = SocketExtension.BeginReceive(socket, buffer, ofs, count, flags, (result) =>
                {
                    int len = SocketExtension.EndReceive(socket, result, out SocketError error);
                    NSJSVirtualMachine machine = socketobject.VirtualMachine;
                    machine.Join((sender, state) =>
                    {
                        if (len > 0)
                        {
                            for (int i = ofs; i < len; i++)
                            {
                                data[i] = buffer[i];
                            }
                        }
                        if (callback != null)
                        {
                            callback.Call(socketobject, NSJSInt32.New(machine, unchecked((int)error)), NSJSInt32.New(machine, len));
                        }
                    });
                })))
                {
                    if (callback != null)
                    {
                        callback.CrossThreading = true;
                    }
                    data.CrossThreading = true;
                }
                return success;
            });
        }

        private static void ReceiveFromAsync(IntPtr info)
        {
            InternalReceiveAsync(info, true, (socket, socketobject, data, buffer, ofs, count, flags, remoteep, callback) =>
            {
                EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                bool success = false;
                if ((success = SocketExtension.BeginReceiveFrom(socket, buffer, ofs, count, ref endpoint, (result) =>
                {
                    int len = SocketExtension.EndReceiveFrom(socket, result, ref endpoint);
                    NSJSVirtualMachine machine = socketobject.VirtualMachine;
                    machine.Join((sender, state) =>
                    {
                        for (int i = ofs; i < len; i++)
                        {
                            data[i] = buffer[i];
                        }
                        ObjectAuxiliary.Fill(ObjectAuxiliary.ToObject(remoteep.VirtualMachine, endpoint), remoteep);
                        if (callback != null)
                        {
                            callback.Call(socketobject, NSJSInt32.New(machine, len));
                        }
                    });
                })))
                {
                    if (callback != null)
                    {
                        callback.CrossThreading = true;
                    }
                    data.CrossThreading = true;
                    remoteep.CrossThreading = true;
                }
                return success;
            });
        }

        private static void InternalReceive(IntPtr info, bool sendto, Func<int, SOCKET, NSJSUInt8Array, byte[], int, int, SocketFlags, NSJSObject, int> recving)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            byte[] buffer = null;
            int len = 0;
            int callmode = 0; // 0：返回缓冲区、1：填充缓冲区
            do
            {
                SocketContext context = GetSocketContext(arguments.This);
                if (context == null)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                NSJSObject socketobject = context.This;
                if (socketobject == null)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                SOCKET socket = context.Socket;
                if (socket == null || SocketExtension.CleanedUp(socket))
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                NSJSObject remoteEP = arguments.Length > 0 ? arguments[0] as NSJSObject : null;
                if ((sendto && remoteEP == null) || (!sendto && remoteEP != null))
                {
                    break;
                }
                int soltdx = 0;
                if (remoteEP != null)
                {
                    soltdx++;
                }
                NSJSInt32 count = arguments.Length > soltdx ? arguments[soltdx] as NSJSInt32 : null;
                if (count != null)
                {
                    NSJSInt32 flags = arguments.Length > (soltdx + 1) ? arguments[soltdx + 1] as NSJSInt32 : null;
                    SocketFlags dw = flags != null ? (SocketFlags)flags.Value : 0;
                    int size = count != null ? count.Value : 0;
                    if (size < 0)
                    {
                        size = 0;
                    }
                    buffer = new byte[size];
                    len = recving(callmode, socket, null, buffer, 0, size, dw, remoteEP);
                }
                else
                {
                    callmode = 1;
                    NSJSUInt8Array cch = arguments.Length > soltdx ? arguments[soltdx] as NSJSUInt8Array : null;
                    if (cch == null)
                    {
                        break;
                    }
                    NSJSInt32 offset = null;
                    NSJSInt32 flags = null;
                    if (arguments.Length == (soltdx + 2))
                    {
                        count = arguments[soltdx + 1] as NSJSInt32;
                    }
                    else if (arguments.Length == (soltdx + 3))
                    {
                        offset = arguments[soltdx + 1] as NSJSInt32;
                        count = arguments[soltdx + 2] as NSJSInt32;
                    }
                    else if (arguments.Length == (soltdx + 4))
                    {
                        offset = arguments[soltdx + 1] as NSJSInt32;
                        count = arguments[soltdx + 2] as NSJSInt32;
                        flags = arguments[soltdx + 3] as NSJSInt32;
                    }
                    int size = count != null ? count.Value : cch.Length;
                    int ofs = offset != null ? offset.Value : 0;
                    SocketFlags dw = flags != null ? (SocketFlags)flags.Value : 0;
                    if (size < 0)
                    {
                        size = 0;
                    }
                    if (ofs < 0)
                    {
                        ofs = 0;
                    }
                    buffer = cch.Buffer;
                    if (buffer == null)
                    {
                        break;
                    }
                    len = recving(callmode, socket, cch, buffer, 0, size, dw, remoteEP);
                }
            } while (false);
            if (callmode == 0)
            {
                if (buffer == null)
                {
                    buffer = BufferExtension.EmptryBuffer;
                }
                arguments.SetReturnValue(buffer, len);
            }
            else if (callmode == 1)
            {
                arguments.SetReturnValue(len);
            }
            else
            {
                throw new NotSupportedException("callmode");
            }
        }

        private static void InternalReceiveAsync(IntPtr info, bool sendto, Func<SOCKET, NSJSObject, NSJSUInt8Array, byte[], int, int, SocketFlags, NSJSObject, NSJSFunction, bool> recving)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            do
            {
                SocketContext context = GetSocketContext(arguments.This);
                if (context == null)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                SOCKET socket = context.Socket;
                if (socket == null || SocketExtension.CleanedUp(socket))
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                NSJSObject remoteEP = arguments.Length > 0 ? arguments[0] as NSJSObject : null;
                if ((sendto && remoteEP == null) || (!sendto && remoteEP != null))
                {
                    break;
                }
                int soltdx = 0;
                if (remoteEP != null)
                {
                    soltdx++;
                }
                NSJSUInt8Array cch = arguments.Length > soltdx ? arguments[soltdx] as NSJSUInt8Array : null;
                if (cch == null)
                {
                    break;
                }
                byte[] buffer = cch.Buffer;
                if (buffer == null)
                {
                    break;
                }
                NSJSInt32 offset = null;
                NSJSInt32 flags = null;
                NSJSInt32 count = null;
                NSJSFunction callback = null;
                if (arguments.Length == (soltdx + 2))
                {
                    count = arguments[soltdx + 1] as NSJSInt32;
                    if (count == null)
                    {
                        callback = arguments[soltdx + 1] as NSJSFunction;
                    }
                }
                else if (arguments.Length == (soltdx + 3))
                {
                    offset = arguments[soltdx + 1] as NSJSInt32;
                    count = arguments[soltdx + 2] as NSJSInt32;
                    if (count == null)
                    {
                        count = offset;
                        offset = null;
                        callback = arguments[soltdx + 2] as NSJSFunction;
                    }
                }
                else if (arguments.Length == (soltdx + 4))
                {
                    offset = arguments[soltdx + 1] as NSJSInt32;
                    count = arguments[soltdx + 2] as NSJSInt32;
                    flags = arguments[soltdx + 3] as NSJSInt32;
                    if (flags == null)
                    {
                        callback = arguments[soltdx + 3] as NSJSFunction;
                    }
                }
                else if (arguments.Length == (soltdx + 5))
                {
                    offset = arguments[soltdx + 1] as NSJSInt32;
                    count = arguments[soltdx + 2] as NSJSInt32;
                    flags = arguments[soltdx + 3] as NSJSInt32;
                    callback = arguments[soltdx + 4] as NSJSFunction;
                    break;
                }
                int size = count != null ? count.Value : cch.Length;
                int ofs = offset != null ? offset.Value : 0;
                SocketFlags dw = flags != null ? (SocketFlags)flags.Value : 0;
                if (size < 0)
                {
                    size = 0;
                }
                if (ofs < 0)
                {
                    ofs = 0;
                }
                success = recving(socket, context.This, cch, buffer, ofs, size, dw, remoteEP, callback);
            } while (false);
            arguments.SetReturnValue(success);
        }

        private static void Connect(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            SOCKET socket = GetSocket(arguments.This);
            bool success = false;
            do
            {
                if (socket == null || SocketExtension.CleanedUp(socket))
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                if (arguments.Length <= 0)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                EndPoint remoteEP = ObjectAuxiliary.ToEndPoint(arguments[0]);
                try
                {
                    if (remoteEP == null)
                    {
                        IPAddress address = ObjectAuxiliary.ToAddress(arguments[0]);
                        if (address == null)
                        {
                            break;
                        }
                        int port = arguments.Length > 1 ? ((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault() : 0;
                        remoteEP = new IPEndPoint(address, port);
                    }
                    if (remoteEP == null)
                    {
                        break;
                    }
                    socket.Connect(remoteEP);
                    success = true;
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            } while (false);
            arguments.SetReturnValue(success);
        }

        private static void Close(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            SocketContext context;
            NSJSKeyValueCollection.Release(arguments.This, out context);
            bool success = false;
            do
            {
                if (context == null)
                {
                    break;
                }
                SocketAsyncEventArgs e = context.AcceptAsync;
                if (e != null)
                {
                    e.Dispose();
                    context.AcceptAsync = null;
                }
                e = context.ConnectedAsync;
                if (e != null)
                {
                    e.Dispose();
                    context.ConnectedAsync = null;
                }
                SOCKET socket = context.Socket;
                context.This = null;
                context.Socket = null;
                context.AcceptAsyncCallback = null;
                context.ConnectedAsyncCallback = null;
                if (socket == null || (success = SocketExtension.CleanedUp(socket)))
                {
                    break;
                }
                SocketExtension.Close(socket);
                success = true;
            } while (false);
            arguments.SetReturnValue(success);
        }

        private static void Bind(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            SOCKET socket = GetSocket(arguments.This);
            do
            {
                if (socket == null || SocketExtension.CleanedUp(socket))
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                if (arguments.Length <= 0)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                EndPoint localEP = ObjectAuxiliary.ToEndPoint(arguments[0]);
                try
                {
                    if (localEP == null)
                    {
                        IPAddress address = ObjectAuxiliary.ToAddress(arguments[0]);
                        if (address == null)
                        {
                            break;
                        }
                        int port = arguments.Length > 1 ? ((arguments[1] as NSJSInt32)?.Value).GetValueOrDefault() : 0;
                        localEP = new IPEndPoint(address, port);
                    }
                    if (localEP == null)
                    {
                        break;
                    }
                    socket.Bind(localEP);
                    success = true;
                }
                catch (Exception e)
                {
                    Throwable.Exception(arguments.VirtualMachine, e);
                }
            } while (false);
            arguments.SetReturnValue(success);
        }

        private static void Send(IntPtr info)
        {
            InternalSend(info, false, (socket, soketobject, buffer, ofs, count, flags, remoteep, callback) => SocketExtension.Send(socket, buffer, ofs, count, flags));
        }

        private static void SendTo(IntPtr info)
        {
            InternalSend(info, true, (socket, socketobject, buffer, ofs, count, flags, remoteep, callback) => SocketExtension.SendTo(socket, buffer, ofs, count, flags, remoteep));
        }

        private static void SendToAsync(IntPtr info)
        {
            InternalSend(info, true, (socket, socketobject, buffer, ofs, count, flags, remoteep, callback) =>
            {
                AsyncCallback callbackt = null;
                NSJSVirtualMachine machine = socketobject.VirtualMachine;
                if (callback != null)
                {
                    callback.CrossThreading = true;
                    callbackt = (result) => machine.Join((sender, state) => callback.Call(socketobject, NSJSInt32.New(machine, SocketExtension.EndSendTo(socket, result))));
                }
                return SocketExtension.BeginSendTo(socket, buffer, ofs, count, remoteep, callbackt);
            });
        }

        private static void SendAsync(IntPtr info)
        {
            InternalSend(info, false, (socket, socketobject, buffer, ofs, count, flags, remoteep, callback) =>
            {
                AsyncCallback callbackt = null;
                NSJSVirtualMachine machine = socketobject.VirtualMachine;
                if (callback != null)
                {
                    callback.CrossThreading = true;
                    callbackt = (result) => machine.Join((sender, state) => callback.Call(socketobject, NSJSInt32.New(machine, SocketExtension.EndSend(socket, result))));
                }
                return SocketExtension.BeginSend(socket, buffer, ofs, count, callbackt);
            });
        }

        private static void InternalSend(IntPtr info, bool sendto, Func<SOCKET, NSJSObject, byte[], int, int, SocketFlags, EndPoint, NSJSFunction, bool> sending)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            do
            {
                SocketContext context = GetSocketContext(arguments.This);
                if (context == null)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                NSJSObject socketobject = context.This;
                if (socketobject == null)
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                SOCKET socket = context.Socket;
                if (socket == null || SocketExtension.CleanedUp(socket))
                {
                    Throwable.ObjectDisposedException(arguments.VirtualMachine);
                    break;
                }
                EndPoint remoteEP = ObjectAuxiliary.ToEndPoint(arguments.Length > 0 ? arguments[0] : null);
                if ((sendto && remoteEP == null) || (!sendto && remoteEP != null))
                {
                    break;
                }
                int soltdx = 0;
                if (remoteEP != null)
                {
                    soltdx++;
                }
                NSJSUInt8Array buffer = arguments.Length > soltdx ? arguments[soltdx] as NSJSUInt8Array : null;
                byte[] cch = buffer?.Buffer;
                if (cch == null)
                {
                    break;
                }
                NSJSInt32 offset = null;
                NSJSInt32 count = null;
                NSJSInt32 flags = null;
                NSJSFunction callback = null;
                if (arguments.Length == (2 + soltdx))
                {
                    count = arguments[1 + soltdx] as NSJSInt32;
                    if (count == null)
                    {
                        callback = arguments[1 + soltdx] as NSJSFunction;
                    }
                }
                else if (arguments.Length == (3 + soltdx))
                {

                    offset = arguments[1 + soltdx] as NSJSInt32;
                    count = arguments[2 + soltdx] as NSJSInt32;
                    if (count == null)
                    {
                        count = offset;
                        offset = null;
                        callback = arguments[2 + soltdx] as NSJSFunction;
                    }
                }
                else if (arguments.Length == (4 + soltdx))
                {
                    offset = arguments[1 + soltdx] as NSJSInt32;
                    count = arguments[2 + soltdx] as NSJSInt32;
                    flags = arguments[3 + soltdx] as NSJSInt32;
                    if (flags == null)
                    {
                        callback = arguments[3 + soltdx] as NSJSFunction;
                    }
                }
                else if (arguments.Length == (5 + soltdx))
                {
                    offset = arguments[1 + soltdx] as NSJSInt32;
                    count = arguments[2 + soltdx] as NSJSInt32;
                    flags = arguments[3 + soltdx] as NSJSInt32;
                    callback = arguments[4 + soltdx] as NSJSFunction;
                    break;
                }
                int ofs = offset != null ? offset.Value : 0;
                SocketFlags dw = flags != null ? (SocketFlags)flags.Value : 0;
                int len = count != null ? count.Value : cch.Length;
                if (len < 0)
                {
                    len = 0;
                }
                if (ofs < 0)
                {
                    ofs = 0;
                }
                success = sending(socket, socketobject, cch, ofs, len, dw, remoteEP, callback);
            } while (false);
            arguments.SetReturnValue(success);
        }

        private static SocketContext GetSocketContext(NSJSObject socket)
        {
            if (socket == null)
            {
                return null;
            }
            return NSJSKeyValueCollection.Get<SocketContext>(socket);
        }

        private static SOCKET GetSocket(NSJSObject socket)
        {
            SocketContext context = GetSocketContext(socket);
            if (context != null)
            {
                return context.Socket;
            }
            return null;
        }

        private static void New(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            AddressFamily addressFamily = AddressFamily.InterNetwork; // AF_INNET
            SocketType socketType = SocketType.Stream; // SOCK_STREAM
            ProtocolType protocolType = ProtocolType.Tcp; // IPPROTO_TCP
            NSJSInt32 i32 = arguments.Length > 0 ? arguments[0] as NSJSInt32 : null;
            if (i32 != null)
            {
                addressFamily = (AddressFamily)i32.Value;
            }
            i32 = arguments.Length > 1 ? arguments[1] as NSJSInt32 : null;
            if (i32 != null)
            {
                socketType = (SocketType)i32.Value;
            }
            i32 = arguments.Length > 2 ? arguments[2] as NSJSInt32 : null;
            if (i32 != null)
            {
                protocolType = (ProtocolType)i32.Value;
            }
            try
            {
                SOCKET socket = new SOCKET(addressFamily, socketType, protocolType);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                if (socketType == SocketType.Dgram)
                {
                    SocketExtension.SioUdpConnectReset(socket);
                }
                arguments.SetReturnValue(New(arguments.VirtualMachine, socket));
            }
            catch (Exception e)
            {
                Throwable.Exception(arguments.VirtualMachine, e);
            }
        }
    }
}
