namespace nsjsdotnet.Core.Net.Web
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;

    public class HttpContext : EventArgs
    {
        private static IDictionary<Thread, HttpContext> contexts = new Dictionary<Thread, HttpContext>();

        public IHttpHandler Handler
        {
            get;
            internal set;
        }

        public HttpRequest Request
        {
            get;
            private set;
        }

        public HttpResponse Response
        {
            get;
            private set;
        }

        public HttpApplication Application
        {
            get;
            private set;
        }

        public static HttpContext Current
        {
            get
            {
                HttpContext context = null;
                lock (contexts)
                {
                    Thread key = Thread.CurrentThread;
                    if (!contexts.TryGetValue(key, out context))
                    {
                        throw new InvalidOperationException();
                    }
                    return context;
                }
            }
        }

        public bool Asynchronous
        {
            get;
            set;
        }

        [SecurityCritical]
        internal HttpContext(HttpApplication application, HttpRequest request, HttpResponse response)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            request.CurrentContext = this;
            response.CurrentContext = this;

            this.Application = application;
            this.Request = request;
            this.Response = response;

            lock (contexts)
            {
                HttpContext context = null;
                Thread key = Thread.CurrentThread;
                if (contexts.TryGetValue(key, out context))
                {
                    contexts[key] = this;
                }
                else
                {
                    contexts.Add(key, this);
                }
            }
        }
    }
}
