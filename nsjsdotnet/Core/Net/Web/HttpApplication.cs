namespace nsjsdotnet.Core.Net.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;

    public class HttpApplication
    {
        private HttpCommunication communication = null;

        public string Name
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }

        public IHttpHandler Handler
        {
            get;
            set;
        }

        public string Root
        {
            get;
            set;
        }

        public event EventHandler<HttpBeginProcessRequestEventArgs> BeginProcessRequest;
        public event EventHandler<HttpContext> EndProcessRequest;

        protected virtual void OnBeginProcessRequest(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            EventHandler<HttpBeginProcessRequestEventArgs> handler = this.BeginProcessRequest;
            bool cancel = false;
            if (handler != null)
            {
                HttpBeginProcessRequestEventArgs args = new HttpBeginProcessRequestEventArgs(this, context);
                handler(this, args);
                cancel = args.Cancel;
            }
            if (!cancel)
            {
                this.OnProcessContext(context);
            }
        }

        protected internal virtual void OnEndProcessRequest(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            EventHandler<HttpContext> handler = this.EndProcessRequest;
            if (handler != null)
            {
                handler(this, context);
            }
        }

        protected virtual void OnProcessContext(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            HttpResponse response = context.Response;
            try
            {
                response.AppendHeader("Server", string.Empty);
                if (!string.IsNullOrEmpty(Name))
                {
                    response.AppendHeader("Servers", Name);
                }
                response.StatusCode = 404;
                HttpFileModule module = new HttpFileModule(this, context);
                if (!module.Handle())
                {
                    IHttpHandler currentHandler = this.Handler;
                    if (currentHandler != null)
                    {
                        context.Handler = currentHandler;
                        response.ContentType = "text/html";
                        response.StatusCode = 200;
                        if (response.ContentEncoding == null)
                        {
                            response.ContentEncoding = Encoding.UTF8;
                        }
                        currentHandler.ProcessRequest(context);
                    }
                    if (context != null && !context.Asynchronous)
                    {
                        response.End();
                    }
                }
            }
            catch (Exception) { /*------------------------------------------A------------------------------------------*/ }
        }

        public void Start(params string[] prefixes)
        {
            this.Start(prefixes as IList<string>);
        }

        public void Start(IList<string> prefixes)
        {
            lock (this)
            {
                if (this.communication != null)
                {
                    throw new InvalidOperationException("this");
                }
                this.communication = new HttpCommunication();
                this.communication.Received = (sender, e) => this.OnReceived(e);
                this.communication.Start(prefixes);
            }
        }

        protected virtual void OnReceived(HttpListenerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.OnBeginProcessRequest(new HttpContext(this, new HttpRequest(context.Request), new HttpResponse(context.Response)));
        }

        public void Stop()
        {
            lock (this)
            {
                if (this.communication != null)
                {
                    this.communication.Stop();
                    this.communication = null;
                }
            }
        }
    }
}
