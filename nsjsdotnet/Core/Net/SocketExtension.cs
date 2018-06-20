namespace nsjsdotnet.Core.Net
{
    using nsjsdotnet.Core.Utilits;
    using System;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public static class SocketExtension
    {
        public const ushort MSS = MTU - 40; // 路由芯片最大分段大小
        public const ushort PPP = 8; // PPP
        public const ushort MTU = 1500 - PPP; // 路由芯片最大传输单元

        public const int SendBufferSize = 524288; // 套接字发送缓冲区大小
        public const int ReceiveBufferSize = 524288; // 套接字接受缓冲区池大小
        public const int BoundaryBufferSize = 1000; // 缓冲区边界大小

        private const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 0x0000000C;
        private const long IOC_IN = 0x80000000;
        private const long IOC_VENDOR = 0x18000000;

        [DllImport("ws2_32.dll", SetLastError = true)]
        private static extern SocketError shutdown([In] IntPtr socketHandle, [In] SocketShutdown how);

        public static bool Shutdown([In] Socket socket)
        {
            return Shutdown(socket, SocketShutdown.Both);
        }

        public static bool Shutdown([In] Socket socket, [In] SocketShutdown how)
        {
            if (Platform.IsWindows())
            {
                return shutdown(socket.Handle, how) == SocketError.Success;
            }
            try
            {
                socket.Shutdown(how);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int SioUdpConnectReset(Socket s, byte[] optionInValue, byte[] optionOutValue)
        {
            return s.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
        }

        public static int SioUdpConnectReset(Socket s)
        {
            return SioUdpConnectReset(s, new byte[4], new byte[] { 0 });
        }

        public static Func<Socket, bool> CleanedUp
        {
            get;
            private set;
        }

        static SocketExtension()
        {
            SocketExtension.CleanedUp = SocketExtension.CompileCleanedUp();
        }

        private static Func<Socket, bool> CompileCleanedUp()
        {
            ParameterExpression s = Expression.Parameter(typeof(Socket), "s");
            Expression<Func<Socket, bool>> e = Expression.Lambda<Func<Socket, bool>>(Expression.OrElse(Expression.Equal(s, Expression.Constant(null)), Expression.Property(s,
                typeof(Socket).GetProperty("CleanedUp", BindingFlags.NonPublic | BindingFlags.Instance))), s);
            return e.Compile();
        }

        public static void Close(Socket s)
        {
            if (s != null)
            {
                Shutdown(s);
                s.Close();
            }
        }

        public static bool BeginSend(Socket s, byte[] buffer, int ofs, int size, AsyncCallback callback)
        {
            SocketError error = SocketError.SocketError;
            try
            {
                if (!SocketExtension.CleanedUp(s))
                {
                    s.BeginSend(buffer, ofs, size, SocketFlags.None, out error, callback, null);
                }
            }
            catch
            {
                return false;
            }
            return error == SocketError.Success;
        }

        public static int EndSend(Socket s, IAsyncResult result)
        {
            SocketError error = SocketError.SocketError;
            int len = 0;
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    len = s.EndSend(result, out error);
                }
                catch
                {
                    len = -1;
                }
            }
            return error != SocketError.Success ? -1 : len;
        }

        public static bool Receive(Socket s, byte[] buffer, int ofs, int size, SocketFlags flags, out int len)
        {
            SocketError error = SocketError.SocketError;
            len = 0;
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    len = s.Receive(buffer, ofs, size, flags, out error);
                }
                catch
                {
                    return false;
                }
            }
            return error == SocketError.Success;
        }

        public static bool ReceiveForm(Socket s, byte[] buffer, int ofs, int size, SocketFlags flags, ref EndPoint remoteEP, out int len)
        {
            len = 0;
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    len = s.ReceiveFrom(buffer, ofs, size, flags, ref remoteEP);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public static bool Receive(Socket s, byte[] buffer, int ofs, int size, out int len)
        {
            return SocketExtension.Receive(s, buffer, ofs, size, 0, out len);
        }

        public static bool BeginReceive(Socket s, byte[] buffer, int ofs, int size, AsyncCallback callback)
        {
            return SocketExtension.BeginReceive(s, buffer, ofs, size, callback, null);
        }

        public static int EndReceive(Socket s, IAsyncResult result)
        {
            SocketError error;
            return SocketExtension.EndReceive(s, result, out error);
        }

        public static int EndReceive(Socket s, IAsyncResult result, out SocketError error)
        {
            error = SocketError.SocketError;
            int len = 0;
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    len = s.EndReceive(result, out error);
                }
                catch (Exception)
                {
                    len = -1;
                }
            }
            return error != SocketError.Success ? -1 : len;
        }

        public static int EndReceiveFrom(Socket s, IAsyncResult result, ref EndPoint remoteEP)
        {
            int len = 0;
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    len = s.EndReceiveFrom(result, ref remoteEP);
                }
                catch (Exception)
                {
                    len = -1;
                }
            }
            return len;
        }

        public static bool BeginReceive(Socket s, byte[] buffer, int ofs, int size, SocketFlags flags, AsyncCallback callback)
        {
            return SocketExtension.BeginReceive(s, buffer, ofs, size, flags, callback, null);
        }

        public static bool BeginReceive(Socket s, byte[] buffer, int ofs, int size, AsyncCallback callback, object state)
        {
            return SocketExtension.BeginReceive(s, buffer, ofs, size, SocketFlags.None, callback, state);
        }

        public static bool BeginReceive(Socket s, byte[] buffer, int ofs, int size, SocketFlags flags, AsyncCallback callback, object state)
        {
            SocketError error = SocketError.SocketError;
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    s.BeginReceive(buffer, ofs, size, flags, out error, callback, state);
                }
                catch
                {
                    return false;
                }
            }
            return error == SocketError.Success;
        }

        public static bool BeginReceiveFrom(Socket s, byte[] buffer, int ofs, int size, ref EndPoint remoteEP, AsyncCallback callback)
        {
            return SocketExtension.BeginReceiveFrom(s, buffer, ofs, size, ref remoteEP, callback, null);
        }

        public static bool BeginSendTo(Socket s, byte[] buffer, int ofs, int size, EndPoint remoteEP, AsyncCallback callback)
        {
            return BeginSendTo(s, buffer, ofs, size, remoteEP, callback, null);
        }

        public static bool BeginSendTo(Socket s, byte[] buffer, int ofs, int size, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    s.BeginSendTo(buffer, ofs, size, SocketFlags.None, remoteEP, callback, state);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public static int EndSendTo(Socket s, IAsyncResult result)
        {
            int len = 0;
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    len = s.EndSendTo(result);
                }
                catch
                {
                    len = -1;
                }
            }
            return len;
        }

        public static bool BeginReceiveFrom(Socket s, byte[] buffer, int ofs, int size, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return SocketExtension.BeginReceiveFrom(s, buffer, ofs, size, SocketFlags.None, ref remoteEP, callback, state);
        }

        public static bool BeginReceiveFrom(Socket s, byte[] buffer, int ofs, int size, SocketFlags flags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    s.BeginReceiveFrom(buffer, ofs, size, flags, ref remoteEP, callback, state);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Receive(Socket s, int size, out byte[] buffer)
        {
            buffer = new byte[size];
            if (!SocketExtension.Receive(s, 0, size, buffer))
            {
                buffer = null;
            }
            return true;
        }

        public static bool Receive(Socket s, int size, byte[] buffer)
        {
            return SocketExtension.Receive(s, 0, size, buffer);
        }

        public static bool Receive(Socket s, int ofs, int size, byte[] buffer)
        {
            try
            {
                if (SocketExtension.CleanedUp(s))
                {
                    return false;
                }
                SocketError error = SocketError.SocketError;
                int cursor = 0;
                while (cursor < size)
                {
                    int len = s.Receive(buffer, (ofs + cursor), (size - cursor), SocketFlags.None, out error);
                    if (len <= 0 || error != SocketError.Success)
                    {
                        return false;
                    }
                    cursor += len;
                }
                if (cursor > 0 && cursor == size && error == SocketError.Success)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool Send(Socket s, byte[] buffer)
        {
            return SocketExtension.Send(s, buffer, 0, buffer.Length);
        }

        public static bool Send(Socket s, byte[] buffer, int len)
        {
            return SocketExtension.Send(s, buffer, 0, len);
        }

        public static bool Send(Socket s, byte[] buffer, int ofs, int size)
        {
            return SocketExtension.Send(s, buffer, ofs, size);
        }

        public static bool Send(Socket s, byte[] buffer, int ofs, int size, SocketFlags flags)
        {
            if (!SocketExtension.CleanedUp(s))
            {
                SocketError error;
                try
                {
                    s.Send(buffer, ofs, size, flags, out error);
                }
                catch (Exception)
                {
                    error = SocketError.SocketError;
                }
                if (error != SocketError.Success)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool SendTo(Socket s, byte[] buffer, EndPoint remoteEP)
        {
            return SocketExtension.SendTo(s, buffer, 0, buffer.Length, remoteEP);
        }

        public static bool SendTo(Socket s, byte[] buffer, int len, EndPoint remoteEP)
        {
            return SocketExtension.SendTo(s, buffer, 0, len, remoteEP);
        }

        public static bool SendTo(Socket s, byte[] buffer, int ofs, int size, EndPoint remoteEP)
        {
            return SocketExtension.SendTo(s, buffer, ofs, size, remoteEP);
        }

        public static bool SendTo(Socket s, byte[] buffer, int ofs, int size, SocketFlags flags, EndPoint remoteEP)
        {
            if (!SocketExtension.CleanedUp(s))
            {
                try
                {
                    s.SendTo(buffer, ofs, size, flags, remoteEP);
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
    }
}
