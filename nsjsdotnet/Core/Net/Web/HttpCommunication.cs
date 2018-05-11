namespace nsjsdotnet.Core.Net.Web
{
    using nsjsdotnet.Core.Threading;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;

    class HttpCommunication
    {
        private GetContextThreadRunnable runnable = null;
        private HttpListener server = null;
        private readonly object locker = new object();

        public Action<object, HttpListenerContext> Received
        {
            get;
            set;
        }

        private class GetContextThreadRunnable : IDisposable
        {
            private bool disposed = false;
            private HttpCommunication communication = null;
            private Thread runnable = null;

            public GetContextThreadRunnable(HttpCommunication communication)
            {
                this.communication = communication;
            }

            ~GetContextThreadRunnable()
            {
                this.Dispose();
            }

            public void Dispose()
            {
                lock (this)
                {
                    this.disposed = false;
                }
                GC.SuppressFinalize(this);
            }

            public void Run()
            {
                lock (this)
                {
                    if (this.disposed)
                    {
                        throw new ObjectDisposedException(typeof(GetContextThreadRunnable).Name);
                    }
                    if (this.runnable == null)
                    {
                        this.runnable = new Thread(this.Handle);
                        this.runnable.Start();
                    }
                }
            }

            private void Handle()
            {
                while (!this.disposed)
                {
                    HttpListenerContext context = null;
                    try
                    {
                        context = this.communication.server.GetContext();
                    }
                    catch (Exception)
                    {
                        Waitable.usleep(25);
                    }
                    Action<object, HttpListenerContext> received = this.communication.Received;
                    if (context != null && received != null)
                    {
                        received(this, context);
                    }
                }
            }
        }

        public void Start(IList<string> prefixes)
        {
            lock (this.locker)
            {
                if (this.server != null)
                {
                    throw new InvalidOperationException();
                }
                this.server = new HttpListener();
                HttpListenerPrefixCollection s = this.server.Prefixes;
                foreach (string prefixe in prefixes)
                {
                    s.Add(prefixe);
                }
                this.server.Start();
                if (this.runnable == null)
                {
                    this.runnable = new GetContextThreadRunnable(this);
                    this.runnable.Run();
                }
            }
        }

        public void Stop()
        {
            lock (this.locker)
            {
                if (this.runnable != null)
                {
                    this.runnable.Dispose();
                    this.runnable = null;
                }
                if (this.server != null)
                {
                    try
                    {
                        this.server.Abort();
                        this.server.Stop();
                    }
                    catch (Exception) { /**/ }
                    this.server = null;
                }
            }
        }
    }
}
