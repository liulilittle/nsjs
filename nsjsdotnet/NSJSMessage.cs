namespace nsjsdotnet
{
    using System;

    public class NSJSMessage : EventArgs
    {
        private NSJSFunctionCallbackInfo arguments;
        private string message;
        private OriginBehavior behavior;

        public enum OriginBehavior : int
        {
            kNone,
            kLog,
            kClear,
            kPrintf,
            kPrintln,
            kMessage,
            kSystem,
        }

        public OriginBehavior Behavior
        {
            get
            {
                return this.behavior;
            }
        }

        public NSJSFunctionCallbackInfo Arguments
        {
            get
            {
                return arguments;
            }
        }

        public bool Cancel
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }

        protected internal NSJSMessage(NSJSFunctionCallbackInfo arguments, OriginBehavior behavior, string message)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            this.arguments = arguments;
            this.message = message;
            this.behavior = behavior;
        }

        public virtual NSJSMessage Post()
        {
            return NSJSMessage.Post(this);
        }

        public static NSJSMessage Post(NSJSMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            NSJSFunctionCallbackInfo arguments = message.arguments;
            NSJSVirtualMachine machine = arguments.VirtualMachine;
            machine.OnMessage(message);
            return message;
        }
    }
}
