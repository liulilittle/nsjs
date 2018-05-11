namespace nsjsdotnet.Core.Hooking
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public class VirtualProxyInvoker : EventArgs
    {
        private readonly object self;
        private readonly object[] s;

        internal VirtualProxyInvoker(object self, Module module, Type type, MethodBase method, object[] s)
        {
            this.self = self;
            this.s = s;
            this.Module = module;
            this.DeclaringType = type;
            this.MethodBase = method;
        }

        public object GetThisPointer()
        {
            return self;
        }

        public object ResultValue
        {
            get;
            set;
        }

        public Module Module
        {
            get;
            private set;
        }

        public Type DeclaringType
        {
            get;
            private set;
        }

        public MethodBase MethodBase
        {
            get;
            private set;
        }

        public object[] GetParameters()
        {
            return s;
        }

        public StackTrace GetStackTrace()
        {
            return new StackTrace();
        }
    }

}
