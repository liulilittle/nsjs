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
            HttpListenerResponse response = e.Response;
            try
            {
                response.AppendHeader("Server", string.Empty);
                if (!string.IsNullOrEmpty(Name))
                {
                    response.AppendHeader("Server-Name", Name);
                }
                response.StatusCode = 404;
                HttpFileModule module = new HttpFileModule(this, e);
                if (!module.Handle())
                {
                    IHttpHandler currentHandler = this.Handler;
                    if (currentHandler == null)
                    {
                        response.Close();
                    }
                    else
                    {
                        HttpContext context = new HttpContext(new HttpRequest(e.Request), new HttpResponse(response)
                        {
                            ContentType = "text/html",
                            StatusCode = 200,
                            ContentEncoding = Encoding.UTF8,
                        })
                        {
                            Handler = currentHandler
                        };
                        currentHandler.ProcessRequest(context);
                    }
                }
            }
            catch (Exception) { /*------------------------------------------A------------------------------------------*/ }
            try
            {
                response.Close();
            }
            catch (Exception) { /*------------------------------------------W------------------------------------------*/ }
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
            this.OnPretreatmentContext(context);
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
