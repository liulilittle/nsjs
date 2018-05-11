namespace nsjsdotnet.Core.Hooking
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class PropertyHooker
    {
        private readonly static IDictionary<Type, PropertyHooker> _hooks = new Dictionary<Type, PropertyHooker>();
        private readonly static object _syncobj = new object();
        private IDictionary<PropertyInfo, PropertyHookContext> _properties;
        private static readonly IDictionary<MethodBase, PropertyHookContext> _map;

        internal static PropertyHookContext GetContext(MethodBase method)
        {
            lock (_syncobj)
            {
                PropertyHookContext context;
                _map.TryGetValue(method, out context);
                return context;
            }
        }

        static PropertyHooker()
        {
            VirtualProxyHooker.ProcessInvoke += ProcessInvoke;
            PropertyHooker._map = new Dictionary<MethodBase, PropertyHookContext>();
        }

        internal class PropertyHookContext
        {
            public MethodSource GetMethodSource;
            public MethodSource SetMethodSource;
            public MethodInfo GetMethodInfo;
            public MethodInfo SetMethodInfo;
            public PropertyInfo PropertyInfo;
        }

        public event EventHandler<PropertyEventArgs> Handling;

        public Type DeclaringType
        {
            get;
            private set;
        }

        private PropertyHooker(Type type)
        {
            this.DeclaringType = type;
            this.AddHook();
        }

        private void AddHook()
        {
            lock (this)
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<PropertyInfo, PropertyHookContext>();
                    Func<MethodInfo, MethodSource> addHook = (mi) =>
                    {
                        if (mi == null || !mi.IsPublic)
                        {
                            return null;
                        }
                        MethodSource source = MethodSource.From(mi);
                        MethodInfo pointcut = VirtualProxyHooker.Create(mi);
                        source.AddHook(pointcut);
                        return source;
                    };
                    foreach (PropertyInfo property in this.DeclaringType.GetProperties())
                    {
                        PropertyHookContext context = new PropertyHookContext();
                        context.PropertyInfo = property;
                        context.GetMethodInfo = property.GetGetMethod();
                        context.SetMethodInfo = property.GetSetMethod();

                        MethodSource source = addHook(context.GetMethodInfo);
                        context.GetMethodSource = source;

                        source = addHook(context.SetMethodInfo);
                        context.SetMethodSource = source;

                        if (context.GetMethodSource != null)
                        {
                            _map.Add(context.GetMethodInfo, context);
                        }
                        if (context.SetMethodSource != null)
                        {
                            _map.Add(context.SetMethodInfo, context);
                        }
                        if (context.GetMethodSource != null || context.SetMethodSource != null)
                        {
                            _properties.Add(property, context);
                        }
                    }
                }
                else
                {
                    Action<MethodSource> resume = (source) =>
                    {
                        if (source != null)
                        {
                            source.Resume();
                        }
                    };
                    foreach (PropertyHookContext context in _properties.Values)
                    {
                        resume(context.SetMethodSource);
                        resume(context.GetMethodSource);
                    }
                }
            }
        }

        private static void ProcessInvoke(VirtualProxyInvoker invoke)
        {
            PropertyHooker hooker = null;
            lock (_syncobj)
            {
                hooker = Get(invoke.DeclaringType);
            }
            if (hooker != null)
            {
                hooker.Handle(invoke);
            }
        }

        private void Handle(VirtualProxyInvoker invoke)
        {
            lock (this)
            {
                this.RemoveHook();
                {
                    PropertyEventArgs property = new PropertyEventArgs(invoke);
                    if (this.Handling != null)
                    {
                        this.Handling(this, property);
                    }
                }
                this.AddHook();
            }
        }

        private bool RemoveHook()
        {
            lock (this)
            {
                if (_properties == null)
                {
                    return false;
                }
                Action<MethodSource> suspend = (source) =>
                {
                    if (source != null)
                    {
                        source.Suspend();
                    }
                };
                foreach (PropertyHookContext context in _properties.Values)
                {
                    suspend(context.SetMethodSource);
                    suspend(context.GetMethodSource);
                }
                return _properties.Count > 0;
            }
        }

        public static PropertyHooker From<T>()
        {
            return From(typeof(T));
        }

        private static PropertyHooker Get(Type type)
        {
            lock (_syncobj)
            {
                PropertyHooker hooker;
                if (_hooks.TryGetValue(type, out hooker))
                {
                    return hooker;
                }
                return hooker;
            }
        }

        public static PropertyHooker From(Type type)
        {
            if (type == null)
            {
                return null;
            }
            lock (_syncobj)
            {
                PropertyHooker hooker = Get(type);
                if (hooker == null)
                {
                    hooker = new PropertyHooker(type);
                    _hooks.Add(type, hooker);
                }
                return hooker;
            }
        }
    }
}
