namespace nsjsdotnet.Core.Hooking
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;

    public static class VirtualProxyHooker
    {
        private static ModuleBuilder DynamicModule
        {
            get;
            set;
        }

        public static event Action<VirtualProxyInvoker> ProcessInvoke;
        private static readonly MethodInfo _onHandle;
        private static readonly IDictionary<MethodInfo, MethodInfo> _mms;
        private static readonly IDictionary<string, Module> _modules;
        private static readonly IDictionary<string, MethodInfo> _methods;
        private static readonly IDictionary<string, Type> _types;
        private static readonly object _looker = new object();

        [SecuritySafeCritical, HostProtection(SecurityAction.Demand)]
        public static object Handle(object obj, string moduleid, int typeid, int methodid, object[] s)
        {
            if (VirtualProxyHooker.ProcessInvoke != null)
            {
                Module m = ResolveModule(moduleid);
                VirtualProxyInvoker invoke = new VirtualProxyInvoker(obj, m, _types[string.Format("{0}.{1}", moduleid, typeid)]
                    , _methods[string.Format("{0}.{1}", moduleid, methodid)], s);
                VirtualProxyHooker.ProcessInvoke(invoke);
                return invoke.ResultValue;
            }
            return null;
        }

        static VirtualProxyHooker()
        {
            VirtualProxyHooker._methods = new Dictionary<string, MethodInfo>();
            VirtualProxyHooker._types = new Dictionary<string, Type>();
            VirtualProxyHooker._mms = new Dictionary<MethodInfo, MethodInfo>();
            VirtualProxyHooker._modules = new Dictionary<string, Module>();
            MethodInfo[] ms = typeof(VirtualProxyHooker).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            Func<Type, MethodInfo> mi = (type) => ms.First(i =>
            {
                var s = i.GetParameters();
                if (s.Length < 5)
                {
                    return false;
                }
                return s[4].ParameterType == type;
            });
            _onHandle = mi(typeof(object[]));
            VirtualProxyHooker.DynamicModule = VirtualProxyHooker.CreateDynamicModule();
        }

        private static ModuleBuilder CreateDynamicModule()
        {
            AssemblyBuilder _asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Path.GetRandomFileName()),
                AssemblyBuilderAccess.Run);
            ModuleBuilder module = _asm.DefineDynamicModule(_asm.FullName, false);
            _asm.DefineVersionInfoResource();
            return module;
        }

        private static TypeBuilder CreateDynamicType()
        {
            return VirtualProxyHooker.DynamicModule.DefineType(Path.GetRandomFileName(), TypeAttributes.Public |
                TypeAttributes.Abstract | TypeAttributes.Sealed, null, null);
        }

        private static void CreateDynamicMethods(Type type, MethodInfo handling)
        {
            MethodInfo[] _mets = type.GetMethods();
            foreach (MethodInfo _met in _mets)
            {
                if (_met.DeclaringType == type)
                {
                    VirtualProxyHooker.CreateDynamicMethod(_met, handling, VirtualProxyHooker.CreateDynamicType());
                }
            }
        }

        private static Type[] GetDynamicParameterType(ParameterInfo[] args)
        {
            Type[] _params = new Type[args.Length + 1];
            for (int i = 1; i <= args.Length; i++)
                _params[i] = args[i - 1].ParameterType;
            _params[0] = typeof(object); return _params;
        }

        [HostProtection(SecurityAction.Demand)]
        private static MethodInfo CreateDynamicMethod(MethodInfo method, MethodInfo handling, TypeBuilder builder)
        {
            ParameterInfo[] s = method.GetParameters();
            MethodBuilder proxy = builder.DefineMethod(builder.Name, MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard, method.ReturnType,
                VirtualProxyHooker.GetDynamicParameterType(s));

            proxy.SetCustomAttribute(new CustomAttributeBuilder(typeof(SecurityCriticalAttribute).
    GetConstructor(Type.EmptyTypes), new object[0]));
            proxy.SetCustomAttribute(new CustomAttributeBuilder(typeof(SecuritySafeCriticalAttribute).
                GetConstructor(Type.EmptyTypes), new object[0]));

              ILGenerator il = proxy.GetILGenerator();
            il.DeclareLocal(typeof(object[]));
            il.DeclareLocal(typeof(object));
            if (method.ReturnType != typeof(void))
            {
                il.DeclareLocal(method.ReturnType);
            }
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldc_I4, s.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_1);
            if (!method.IsStatic)
            {
                Label rva = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brtrue, rva);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stloc_1);
                il.MarkLabel(rva);
            }
            for (int o = -1, i = (method.IsStatic ? 0 : 1),
                n = (method.IsStatic ? s.Length - 1 : s.Length); i <= n; i++)
            {
                o = (i - 1);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldc_I4, o);
                il.Emit(OpCodes.Ldarg, i);
                il.Emit(OpCodes.Box, s[o].ParameterType);
                il.Emit(OpCodes.Stelem_Ref);
            }
            string moduleid = method.Module.ModuleVersionId.ToString();
            string typeid = string.Format("{0}.{1}", moduleid, method.DeclaringType.MetadataToken);
            string methodid = string.Format("{0}.{1}", moduleid, method.MetadataToken);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldstr, moduleid);
            il.Emit(OpCodes.Ldc_I4, method.DeclaringType.MetadataToken);
            il.Emit(OpCodes.Ldc_I4, method.MetadataToken);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call, handling);
            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Pop);
            }
            else
            {
                il.Emit(OpCodes.Unbox_Any, method.ReturnType);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloc_1);
            }
            il.Emit(OpCodes.Ret);
            if (!_modules.ContainsKey(moduleid))
            {
                _modules.Add(moduleid, method.Module);
            }
            if (!_types.ContainsKey(typeid))
            {
                _types.Add(typeid, method.DeclaringType);
            }
            if (!_methods.ContainsKey(methodid))
            {
                _methods.Add(methodid, method);
            }
            return builder.CreateType().GetMethod(builder.Name);
        }

        private static MethodInfo Create(MethodInfo method, MethodInfo handing)
        {
            lock (_looker)
            {
                MethodInfo mi;
                if (_mms.TryGetValue(method, out mi))
                {
                    return mi;
                }
                TypeBuilder tb = VirtualProxyHooker.CreateDynamicType();
                mi = VirtualProxyHooker.CreateDynamicMethod(method, handing, tb);
                _mms.Add(method, mi);
                return mi;
            }
        }

        public static MethodInfo Create(MethodInfo method)
        {
            return Create(method, _onHandle);
        }

        public static Module ResolveModule(string moduleid)
        {
            if (string.IsNullOrEmpty(moduleid))
            {
                return null;
            }
            lock (_looker)
            {
                Module mm;
                if (_modules.TryGetValue(moduleid, out mm))
                    return mm;
                else
                {
                    AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetModules().
                        FirstOrDefault(m => ((m.ModuleVersionId.ToString() == moduleid) ? mm = m : mm = null) != null) != null);
                    _modules.Add(moduleid, mm);
                    return mm;
                }
            }
        }
    }

}
