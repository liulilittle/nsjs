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

        public event EventHandler<HttpPretreatmentContext> PretreatmentContext;

        protected virtual void OnPretreatmentContext(HttpListenerContext e)
        {
            do
            {
                if (e == null)
                {
                    break;
                }
                EventHandler<HttpPretreatmentContext> handler = this.PretreatmentContext;
                bool cancel = false;
                if (handler != null)
                {
                    HttpPretreatmentContext context = new HttpPretreatmentContext(this, e);
                    handler(this, context);
                    cancel = context.Cancel;
                }
                if (cancel)
                {
                    break;
                }
                this.OnProcessContext(e);
            } while (false);
        }

        protected virtual void OnProcessContext(HttpListenerContext e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            try
            {
                e.Response.Headers.Add(HttpResponseHeader.Server, string.Empty);
                if (!string.IsNullOrEmpty(Name))
                {
                    e.Response.AppendHeader("Server-Name", Name);
                }
                e.Response.StatusCode = 404;
                HttpFileModule module = new HttpFileModule(this, e);
                if (!module.Handle())
                {
                    IHttpHandler currentHandler = this.Handler;
                    if (currentHandler == null)
                    {
                        e.Response.Close();
                    }
                    else
                    {
                        HttpContext context = new HttpContext(new HttpRequest(e.Request), new HttpResponse(e.Response)
                        {
                            ContentType = "text/html",
                            StatusCode = 200,
                            ContentEncoding = Encoding.UTF8,
                        })
                        {
                            Handler = currentHandler
                        };
                        currentHandler.ProcessRequest(context);
                        context.Response.End();
                    }
                }
            }
            catch (Exception) { /**/ }
        }

        public void Start(params string[] prefixes)
        {
            Start(prefixes as IList<string>);
        }

        public void Start(IList<string> prefixes)
        {
            lock (this)
            {
                if (communication != null)
                {
                    throw new InvalidOperationException();
                }
                communication = new HttpCommunication();
                communication.Received = Received;
                communication.Start(prefixes);
            }
        }

        private void Received(object sender, HttpListenerContext context)
        {
            this.OnReceived(context);
        }

        protected virtual void OnReceived(HttpListenerContext context)
        {
            OnPretreatmentContext(context);
        }

        public void Stop()
        {
            lock (this)
            {
                if (communication != null)
                {
                    communication.Stop();
                    communication = null;
                }
            }
        }
    }
}
