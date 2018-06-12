namespace nsjsdotnet.Core.Net.WebSocket
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    public class WebSocket
    {
        private WebSocketSessionHandshake sessionHandshake;
        private WebSocketServerHandshake serverHandshake;
        private byte[] _buffer;
        private bool clientMode;
        private Uri connectUri;
        private bool cleanUp;
        private IPEndPoint serverEP;
        private WebSocketFrame frame;
        private readonly Socket socket;
        private static readonly Encoding encoding = Encoding.UTF8;

        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<EventArgs> OnOpen;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<CloseEventArgs> OnClose;

        public bool Available
        {
            get
            {
                lock (this)
                {
                    if (SocketExtension.CleanedUp(socket))
                    {
                        return false;
                    }
                    return socket.Connected;
                }
            }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                return socket.LocalEndPoint;
            }
        }

        public int Ttl
        {
            get
            {
                return socket.Ttl;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return socket.Handle;
            }
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                return socket.RemoteEndPoint;
            }
        }

        public string Path
        {
            get
            {
                if (clientMode)
                {
                    return connectUri.AbsolutePath;
                }
                else
                {
                    return serverHandshake.RawUri;
                }
            }
        }

        public object Tag
        {
            get;
            set;
        }

        public object UserToken
        {
            get;
            set;
        }

        public WebSocket(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException("uri");
            }
            Uri url = new Uri(uri);
            if (url.Scheme != "ws")
            {
                throw new ArgumentException("uri");
            }
            if (string.IsNullOrEmpty(url.LocalPath))
            {
                throw new ArgumentNullException("uri");
            }
            if (url.LocalPath[0] != '/')
            {
                throw new ArgumentNullException("uri");
            }
            IPEndPoint server = new IPEndPoint(IPAddress.Parse(url.Host), url.Port);
            this.clientMode = true;
            this.connectUri = url;
            this.serverEP = server;
            this.socket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Open()
        {
            if (this.Available)
            {
                if (clientMode)
                {
                    throw new InvalidOperationException();
                }
            }
            lock (this)
            {
                if (clientMode)
                {
                    this.DoConnectAsync(this.serverEP);
                }
                else
                {
                    SHCreateThread(this.Handshake);
                }
            }
        }

        private void DoConnectAsync(IPEndPoint server)
        {
            EventHandler<SocketAsyncEventArgs> handler = (sender, evt) =>
            {
                if (evt.SocketError != SocketError.Success)
                {
                    CloseOrError(true);
                }
                else
                {
                    SHCreateThread(Handshake);
                }
            };
            try
            {
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.Completed += handler;
                e.RemoteEndPoint = server;
                if (!socket.ConnectAsync(e))
                {
                    handler(socket, e);
                }
            }
            catch (Exception)
            {
                CloseOrError(true);
            }
        }

        [SecurityCritical, SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public WebSocket(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (!socket.Connected)
            {
                throw new SocketException();
            }
            this.socket = socket;
            this.clientMode = false;
        }

        private static void SHCreateThread(ThreadStart startRoute)
        {
            Thread main = new Thread(startRoute);
            main.IsBackground = true;
            main.Priority = ThreadPriority.Lowest;
            main.Start();
        }

        public void Close()
        {
            CloseOrError(false);
        }

        public bool NoDelay
        {
            get
            {
                return socket.NoDelay;
            }
            set
            {
                socket.NoDelay = true;
            }
        }

        private void CloseOrError(bool error)
        {
            bool doEvent = false;
            lock (this)
            {
                if (!cleanUp)
                {
                    doEvent = !cleanUp;
                    SocketExtension.Close(socket);
                    cleanUp = true;
                }
            }
            if (doEvent)
            {
                if (error)
                {
                    DoError(new ErrorEventArgs());
                }
                else
                {
                    DoClose(new CloseEventArgs());
                }
            }
        }

        protected virtual void DoError(ErrorEventArgs e)
        {
            if (OnError != null)
            {
                OnError(this, e);
            }
        }

        protected virtual void DoClose(CloseEventArgs e)
        {
            if (OnClose != null)
            {
                OnClose(this, e);
            }
        }

        protected virtual void DoOpen(EventArgs e)
        {
            if (OnOpen != null)
            {
                OnOpen(this, e);
            }
        }

        private void Handshake()
        {
            lock (this)
            {
                if (clientMode)
                {
                    sessionHandshake = WebSocketSessionHandshake.Handshake(this,
                        socket, connectUri);
                }
                else
                {
                    serverHandshake = WebSocketServerHandshake.Handshake(this, socket);
                }
            }
            if (serverHandshake == null && sessionHandshake == null)
            {
                CloseOrError(true);
            }
            else
            {
                DoOpen(EventArgs.Empty);
                ProcessReceive(null);
            }
        }

        private unsafe void ProcessReceive(IAsyncResult result)
        {
            if (_buffer == null)
            {
                _buffer = new byte[SocketExtension.MSS];
            }
            if (result == null)
            {
                int len = SocketExtension.MSS;
                if (frame != null)
                {
                    long surplus = frame.payload_surplus;
                    if (surplus < len)
                    {
                        len = (int)surplus;
                    }
                }
                if (!SocketExtension.BeginReceive(socket, _buffer, 0, len,
                        ProcessReceive))
                {
                    CloseOrError(true);
                }
            }
            else
            {
                int len = SocketExtension.EndReceive(socket, result);
                if (len <= 0)
                {
                    CloseOrError(false);
                }
                else
                {
                    fixed (byte* pinned = _buffer)
                    {
                        ProcessReceive(pinned, len);
                    }
                }
            }
        }

        private unsafe void ProcessReceive(byte* buffer, int len)
        {
            bool error = false;
            do
            {
                if (this.frame == null)
                {
                    this.frame = WebSocketFrame.Unpack(buffer, len);
                    if (this.frame == null)
                    {
                        error = true;
                        break;
                    }
                }
                else
                {
                    WebSocketFrame.PayloadAdditional(frame, buffer, len);
                }
                long surplus = this.frame.payload_surplus;
                if (surplus <= 0)
                {
                    WebSocketFrame frame = this.frame;
                    this.frame = null;
                    ProcessFrame(frame);
                }
                ProcessReceive(null);
            } while (false);
            if (error)
            {
                CloseOrError(true);
            }
        }

        private unsafe void ProcessFrame(WebSocketFrame frame)
        {
            if (frame == null || !frame.fin) // 不支持多个分片帧
            {
                CloseOrError(true);
            }
            else
            {
                DoMessage(new MessageEventArgs((OpCode)frame.opcode, frame.payload_data));
            }
        }

        protected virtual void DoMessage(MessageEventArgs e)
        {
            if (OnMessage != null)
            {
                OnMessage(this, e);
            }
        }

        public bool Send(string message)
        {
            if (message == null)
            {
                return false;
            }
            lock (this)
            {
                if (!socket.Connected)
                {
                    return false;
                }
                byte[] buffer;
                if (message.Length <= 0)
                {
                    buffer = BufferExtension.EmptryBuffer;
                }
                else
                {
                    buffer = encoding.GetBytes(message);
                }
                return Send(OpCode.Text, buffer);
            }
        }

        public bool Send(byte[] buffer)
        {
            return Send(OpCode.Binary, buffer);
        }

        public bool Send(OpCode opcode, byte[] buffer)
        {
            if (buffer == null)
            {
                return false;
            }
            lock (this)
            {
                if (!socket.Connected)
                {
                    return false;
                }
                WebSocketFrame frame = new WebSocketFrame();
                frame.opcode = (byte)opcode; 
                frame.fin = true;
                frame.rsv1 = false;
                frame.rsv2 = false;
                frame.rsv3 = false;
                frame.masked = clientMode; // 客户端需要加密；服务器禁止加密（chrome）
                frame.payload_data = buffer;
                frame.payload_length = buffer.Length;
                using (MemoryStream s = WebSocketFrame.Pack(frame))
                {
                    return SocketExtension.BeginSend(socket, s.GetBuffer(), 0, (int)s.Position, null);
                }
            }
        }
    }
}
