namespace nsjsdotnet
{
    using System;

    public class NSJSMessage : EventArgs
    {
        public NSJSValue Message
        {
            get;
            private set;
        }

        public NSJSFunctionCallbackInfo Arguments
        {
            get;
            private set;
        }

        public NSJSMessage(NSJSFunctionCallbackInfo arguments, NSJSValue message)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            this.Message = message;
            this.Arguments = arguments;
        }
    }
}
