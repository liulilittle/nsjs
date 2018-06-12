namespace nsjsdotnet.Core.Net.WebSocket
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class WebSocketServer
    {
        private Socket server;
        private int port;

        public event OpenEventHandler OnOpen;
        public event MessageEventHandler OnMessage;
        public event ErrorEventHandler OnError;
        public event CloseEventHandler OnClose;

        public WebSocketServer(int port)
        {
            if (port <= 0 || port > 0xFFFF)
            {
                throw new ArgumentException("port");
            }
            this.port = port;
        }

        public void Start()
        {
            lock (this)
            {
                if (server != null)
                {
                    throw new InvalidOperationException();
                }
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                server.Bind(new IPEndPoint(IPAddress.Any, port));
                server.Listen(int.MaxValue);
                StartAccept(null);
            }
        }

        public int Port
        {
            get
            {
                return port;
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

        public int GetBindPort()
        {
            if (server == null)
            {
                return 0;
            }
            IPEndPoint ipep = (IPEndPoint)server.LocalEndPoint;
            return ipep.Port;
        }

        public void Stop()
        {
            lock (this)
            {
                if (server != null)
                {
                    SocketExtension.Close(server);
                    server = null;
                }
            }
        }

        private void StartAccept(SocketAsyncEventArgs e)
        {
            bool willRaiseEvent = true;
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += ProcessAccept;
            }
            e.AcceptSocket = null;
            try
            {
                lock (this)
                {
                    if (!SocketExtension.CleanedUp(server))
                    {
                        willRaiseEvent = server.AcceptAsync(e);
                    }
                }
            }
            catch (Exception) { /*-A-*/ }
            if (!willRaiseEvent)
            {
                ProcessAccept(server, e);
            }
        }

        private void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                WebSocket ws = new WebSocket(e.AcceptSocket);
                ws.OnClose += WebSocket_OnClose;
                ws.OnError += WebSocket_OnError;
                ws.OnMessage += WebSocket_OnMessage;
                ws.OnOpen += WebSocket_OnOpen;
                ws.Open();
            }
            StartAccept(e);
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            if (OnOpen != null)
            {
                OnOpen((WebSocket)sender, e);
            }
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            if (OnMessage != null)
            {
                OnMessage((WebSocket)sender, e);
            }
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            if (OnError != null)
            {
                OnError((WebSocket)sender, e);
            }
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            if (OnClose != null)
            {
                OnClose((WebSocket)sender, e);
            }
        }
    }
}
