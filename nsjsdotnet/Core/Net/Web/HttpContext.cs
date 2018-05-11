namespace nsjsdotnet.Core.Net.Web
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;

    public class HttpContext
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

        [SecurityCritical]
        internal HttpContext(HttpRequest request, HttpResponse response)
        {
            request.CurrentContext = this;
            response.CurrentContext = this;

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
