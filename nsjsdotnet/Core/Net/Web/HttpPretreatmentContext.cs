namespace nsjsdotnet.Core.Net.Web
{
    using System;
    using System.Net;

    public class HttpPretreatmentContext : EventArgs
    {
        public HttpApplication Application
        {
            get;
            private set;
        }

        public HttpListenerContext CurrentContext
        {
            get;
            private set;
        }

        public bool Cancel
        {
            get;
            set;
        }

        public HttpPretreatmentContext(HttpApplication application, HttpListenerContext context)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.CurrentContext = context;
            this.Application = application;
        }
    }
}
