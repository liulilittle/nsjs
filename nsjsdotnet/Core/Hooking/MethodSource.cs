namespace nsjsdotnet.Core.Hooking
{
    using System;
    using System.Security.Permissions;
    using System.Diagnostics;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Security;
    using System.Runtime.InteropServices;

    public sealed class MethodSource
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static IDictionary<MethodInfo, MethodSource> _sources;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly object _syncobj = new object();

        private readonly MethodInfo _method;
        private NetHook _hook;
        private readonly IntPtr _methodPtr;

        private MethodSource(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            _method = method;
            _methodPtr = method.MethodHandle.GetFunctionPointer();
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        static MethodSource()
        {
            _sources = new Dictionary<MethodInfo, MethodSource>();
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public bool AddHook(MethodInfo handing)
        {
            if (handing == null)
            {
                return false;
            }
            lock (this)
            {
                RuntimeMethodHandle handle = handing.MethodHandle;
                IntPtr address = handle.GetFunctionPointer();
                return AddHook(address);
            }
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public bool AddHook(Delegate d)
        {
            if (d == null)
            {
                return false;
            }
            lock (this)
            {
                IntPtr address = Marshal.GetFunctionPointerForDelegate(d);
                return AddHook(address);
            }
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public void Suspend()
        {
            lock (this)
            {
                _hook.Suspend();
            }
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public void Resume()
        {
            lock (this)
            {
                _hook.Resume();
            }
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public void Invoke(Action block)
        {
            if (block == null)
            {
                throw new ArgumentNullException("block");
            }
            lock (this)
            {
                this.Suspend();
                block();
                this.Resume();
            }
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public T Invoke<T>(Func<T> block)
        {
            if (block == null)
            {
                throw new ArgumentNullException("block");
            }
            lock (this)
            {
                this.Suspend();
                T result = block();
                this.Resume();
                return result;
            }
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        private bool AddHook(IntPtr address)
        {
            lock (this)
            {
                if (address == IntPtr.Zero)
                {
                    return false;
                }
                if (_hook != null)
                {
                    return false;
                }
                _hook = NetHook.CreateManaged(_methodPtr, address);
                return true;
            }
        }

        public MethodInfo Method
        {
            get
            {
                return _method;
            }
        }

        [SecuritySafeCritical, HostProtection(Action = SecurityAction.Demand)]
        public static MethodSource From(MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }
            lock (_syncobj)
            {
                MethodSource source;
                if (!_sources.TryGetValue(method, out source))
                {
                    source = new MethodSource(method);
                    _sources.Add(method, source);
                }
                return source;
            }
        }
    }
}
