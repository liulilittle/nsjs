namespace nsjsdotnet.Core.Net.Web
{
    using System;

    public class HttpBeginProcessRequestEventArgs : EventArgs
    {
        public HttpApplication Application
        {
            get;
            private set;
        }

        public HttpContext CurrentContext
        {
            get;
            private set;
        }

        public bool Cancel
        {
            get;
            set;
        }

        public HttpBeginProcessRequestEventArgs(HttpApplication application, HttpContext context)
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
