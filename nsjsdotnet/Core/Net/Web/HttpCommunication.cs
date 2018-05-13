namespace nsjsdotnet.Core.Net.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    class HttpCommunication
    {
        private HttpListener server = null;
        private readonly object locker = new object();

        public Action<object, HttpListenerContext> Received
        {
            get;
            set;
        }

        private void InternalGetContextAsyncCycleLooper(IAsyncResult result)
        {
            do
            {
                if (server == null)
                {
                    break;
                }
                else if (result == null)
                {
                    try
                    {
                        server.BeginGetContext(InternalGetContextAsyncCycleLooper, null);
                    }
                    catch (Exception) { break; }
                }
                else
                {
                    try
                    {
                        HttpListenerContext context = server.EndGetContext(result);
                        if (context == null)
                        {
                            break;
                        }
                        Action<object, HttpListenerContext> received = this.Received;
                        if (received != null)
                        {
                            received(this, context);
                        }
                    }
                    catch (Exception) { break; }
                    this.InternalGetContextAsyncCycleLooper(null);
                }
            } while (false);
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
                this.InternalGetContextAsyncCycleLooper(null);
            }
        }

        public void Stop()
        {
            lock (this.locker)
            {
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
