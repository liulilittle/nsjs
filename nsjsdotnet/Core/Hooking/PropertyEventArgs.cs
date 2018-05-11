namespace nsjsdotnet.Core.Hooking
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public class PropertyEventArgs : EventArgs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PropertyInfo property;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private VirtualProxyInvoker invoke;

        public PropertyEventArgs(VirtualProxyInvoker invoke)
        {
            if (invoke == null)
            {
                throw new ArgumentNullException("invoke");
            }
            this.invoke = invoke;
        }

        public PropertyInfo PropertyInfo
        {
            get
            {
                lock (this)
                {
                    if (property == null)
                    {
                        property = PropertyHooker.GetContext(MethodBase).
                            PropertyInfo;
                    }
                    return property;
                }
            }
        }

        public bool IsGetValue
        {
            get
            {
                return PropertyInfo.GetGetMethod() == MethodBase;
            }
        }

        public void SetValue(object value, object[] index)
        {
            object obj = GetThisPointer();
            PropertyInfo.SetValue(obj, value, index);
        }

        public object GetValue(object[] index)
        {
            object obj = GetThisPointer();
            return PropertyInfo.GetValue(obj, index);
        }

        public object GetThisPointer()
        {
            return invoke.GetThisPointer();
        }

        public object ResultValue
        {
            get
            {
                return invoke.ResultValue;
            }
            set
            {
                invoke.ResultValue = value;
            }
        }

        public Module Module
        {
            get
            {
                return invoke.Module;
            }
        }

        public Type DeclaringType
        {
            get
            {
                return invoke.DeclaringType;
            }
        }

        public MethodBase MethodBase
        {
            get
            {
                return invoke.MethodBase;
            }
        }

        public object[] GetParameters()
        {
            return invoke.GetParameters();
        }

        public StackTrace GetStackTrace()
        {
            return invoke.GetStackTrace();
        }
    }
}
