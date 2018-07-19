namespace nsjsdotnet.Core
{
    using nsjsdotnet;
    using nsjsdotnet.Core.Utilits;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Mail;
    using System.Reflection;

    public class SimpleAgent
    {
        private static string K<T>(string name)
        {
            return K(typeof(T), name);
        }

        private static string K(Type type, string name)
        {
            string key = name;
            if (type != null)
            {
                key = type.FullName + "." + name;
            }
            return key;
        }

        private static TResult Get<TType, TResult>(string name) where TResult : MemberInfo
        {
            return Get<TResult>(typeof(TType), name);
        }

        private static TResult Get<TResult>(Type type, string name) where TResult : MemberInfo
        {
            string key = K(type, name);
            return miTable[key] as TResult;
        }

        static SimpleAgent()
        {
            miToValueTable.Keys.FirstOrDefault(i => InternalCheckConverter(i) == null);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly IDictionary<string, MemberInfo> miTable = new Dictionary<string, MemberInfo>()
        {
            { K(typeof(NSJSFunctionCallbackInfo), "From"), typeof(NSJSFunctionCallbackInfo).GetMethod("From", new[] { typeof(IntPtr) }) },
            { K(typeof(NSJSKeyValueCollection), "Get"), typeof(NSJSKeyValueCollection).GetMethods().First(m => !m.IsGenericMethod && m.Name == "Get" && m.GetParameters().FirstOrDefault(i => i.ParameterType == typeof(NSJSObject)) != null) },
        };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly IDictionary<Type, object> miToValueTable = new Dictionary<Type, object>()
        {
            { typeof(int),  GN2XV(typeof(ValueAuxiliary).GetMethod("ToInt32", new Type[] { typeof(NSJSValue) })) },
            { typeof(uint),  GN2XV(typeof(ValueAuxiliary).GetMethod("ToUInt32", new Type[] { typeof(NSJSValue) })) },
            { typeof(short),  GN2XV(typeof(ValueAuxiliary).GetMethod("ToInt16", new Type[] { typeof(NSJSValue) })) },
            { typeof(ushort), GN2XV(typeof(ValueAuxiliary).GetMethod("ToUInt16", new Type[] { typeof(NSJSValue) })) },
            { typeof(sbyte), GN2XV(typeof(ValueAuxiliary).GetMethod("ToSByte", new Type[] { typeof(NSJSValue) })) },
            { typeof(byte), GN2XV(typeof(ValueAuxiliary).GetMethod("ToByte", new Type[] { typeof(NSJSValue) })) },
            { typeof(char), GN2XV(typeof(ValueAuxiliary).GetMethod("ToChar", new Type[] { typeof(NSJSValue) })) },
            { typeof(float), GN2XV(typeof(ValueAuxiliary).GetMethod("ToSingle", new Type[] { typeof(NSJSValue) })) },
            { typeof(double), GN2XV(typeof(ValueAuxiliary).GetMethod("ToDouble", new Type[] { typeof(NSJSValue) })) },
            { typeof(long), GN2XV(typeof(ValueAuxiliary).GetMethod("ToInt64", new Type[] { typeof(NSJSValue) })) },
            { typeof(ulong), GN2XV(typeof(ValueAuxiliary).GetMethod("ToUInt64", new Type[] { typeof(NSJSValue) })) },
            { typeof(bool), GN2XV(typeof(ValueAuxiliary).GetMethod("ToBoolean", new Type[] { typeof(NSJSValue) })) },
            { typeof(string), GN2XV(typeof(ValueAuxiliary).GetMethod("ToString", new Type[] { typeof(NSJSValue) })) },
            { typeof(decimal), GN2XV(typeof(ValueAuxiliary).GetMethod("ToDecimal", new Type[] { typeof(NSJSValue) })) },
            { typeof(DateTime), GN2XV(typeof(ValueAuxiliary).GetMethod("ToDateTime", new Type[] { typeof(NSJSValue) })) },
            { typeof(object), new Func<NSJSValue, object>((value) =>
                              {
                                  NSJSObject o = value as NSJSObject;
                                  if (o == null)
                                  {
                                      return value.GetValue();
                                  }
                                  if (!NSJSKeyValueCollection.Contains(o))
                                  {
                                     return value;
                                  }
                                  return NSJSKeyValueCollection.Get(o);
                             })
            },
            { typeof(IPAddress), GN2XV(typeof(ObjectAuxiliary).GetMethod("ToAddress", new Type[] { typeof(NSJSValue) })) },
            { typeof(EndPoint), GN2XV(typeof(ObjectAuxiliary).GetMethod("ToEndPoint", new Type[] { typeof(NSJSValue) })) },
            { typeof(Cookie), GN2XV(typeof(ObjectAuxiliary).GetMethod("ToCookie", new Type[] { typeof(NSJSValue) })) },
            { typeof(Attachment), GN2XV(typeof(ObjectAuxiliary).GetMethod("ToAttachment", new Type[] { typeof(NSJSValue) })) },
            { typeof(MailMessage), GN2XV(typeof(ObjectAuxiliary).GetMethod("ToMailMessage", new Type[] { typeof(NSJSValue) })) },
            { typeof(MailAddress), GN2XV(typeof(ObjectAuxiliary).GetMethod("ToMailAddress", new Type[] { typeof(NSJSValue) })) },
        };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly IDictionary<Type, Func<NSJSValue, object>> miToValueConverterTable = new Dictionary<Type, Func<NSJSValue, object>>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly IDictionary<CompilerCacheKey, NSJSFunctionCallback> fnCompilerCaches = new Dictionary<CompilerCacheKey, NSJSFunctionCallback>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly IDictionary<Type, IDictionary<string, MemberInfo>> miToValueKeyMemberTable = new Dictionary<Type, IDictionary<string, MemberInfo>>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly object syncobj = new object();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly static IDictionary<Type, NetToObjectCallables> netToObjectCallablesTable = new Dictionary<Type, NetToObjectCallables>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Func<NSJSFunctionCallbackInfo, int, NSJSValue> FCIGTITEM = (arguments, index) =>
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguemnts");
            }
            NSJSVirtualMachine machine = arguments.VirtualMachine;
            if (index < 0 || index >= arguments.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            else
            {
                return arguments[index];
            }
        };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Action<NSJSFunctionCallbackInfo, Action<NSJSFunctionCallbackInfo>> JSINVKHANDLING = (arguments, e) =>
        {
            if (arguments == null)
            {
                throw new InvalidProgramException("The arguments parameter is one that cannot be null, but its value is null");
            }
            try
            {
                e(arguments);
            }
            catch (Exception exception)
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly NSJSFunctionCallback FDEFAULTDISPOSE = NSJSPinnedCollection.Pinned<NSJSFunctionCallback>((info) =>
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments != null)
            {
                System.IDisposable disposable;
                NSJSKeyValueCollection.Release(arguments.This, out disposable);
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        });

        private struct CompilerCacheKey
        {
            public Type type;
            public MethodInfo method;
        }

        private static object GN2XV(MethodInfo m)
        {
            Type ft = Expression.GetFuncType(typeof(NSJSValue), m.ReturnType);
            return ft.GetConstructors()[0].Invoke(new object[] { null, m.MethodHandle.GetFunctionPointer() });
        }

        private static Expression Throw(Exception exception)
        {
            return Throw(Expression.Constant(exception));
        }

        private static Expression Throw(Expression exception)
        {
            return Expression.Throw(exception);
        }

        private static IEnumerable<Expression> ComplierBlock(MethodInfo m, ParameterExpression arguments, ParameterExpression self,
            IList<ParameterExpression> localvar)
        {
            IList<Expression> expressions = new List<Expression>()
            {
                Expression.IfThen(Expression.LessThan(Expression.Property(arguments, "Length"),
                    Expression.Constant(localvar.Count)),
                    Throw(new ArgumentOutOfRangeException("JavaScript does not provide a sufficient number of parameters"))),

                Expression.Assign(self, Expression.TypeAs(Expression.Call(Get<MethodInfo>(typeof(NSJSKeyValueCollection), "Get"),
                    new[] { Expression.Property(arguments, "This") }), m.DeclaringType)),
            };
            if (!m.IsStatic)
            {
                expressions.Add(Expression.IfThen(Expression.Equal(self, Expression.Constant(null)),
                    Throw(new InvalidOperationException("This JavaScript object does not have a pointer associated with this"))));
            }
            for (int slot = 0; slot < localvar.Count; slot++)
            {
                ParameterExpression key = localvar[slot];
                Type type = key.Type;
                if (type == typeof(NSJSFunctionCallbackInfo))
                {
                    expressions.Add(Expression.Assign(key, arguments));
                }
                else
                {
                    Expression get = Expression.Call(Expression.Constant(FCIGTITEM), "Invoke", null,
                        new Expression[] { arguments, Expression.Constant(slot) });

                    Expression invk = Expression.Call(Expression.Constant(InternalCheckConverter(type)), "Invoke",
                        null, new Expression[] { get });
                    expressions.Add(Expression.Assign(key, invk));
                }
            }
            expressions.Add(ComplierCalleeDelegateTarget(m, self, arguments, localvar));
            return expressions;
        }

        private static object InternalCheckConverter(Type type, int mode = 0)
        {
            object converter = null;
            lock (syncobj)
            {
                if (!miToValueTable.TryGetValue(type, out converter))
                {
                    if (mode == 1)
                    {
                        return null;
                    }
                    ConstructorInfo constructor = type.GetConstructors().FirstOrDefault(i => i.GetParameters().Length <= 0);
                    if (constructor == null)
                    {
                        throw new ArgumentException("Type does not have any constructor that does not have the parameter to be directly new");
                    }
                    Func<NSJSValue, object> SV2O = (Func<NSJSValue, object>)miToValueTable[typeof(object)];
                    Func<NSJSValue, object> FV2O = new Func<NSJSValue, object>((value) =>
                    {
                        object r = SV2O(value);
                        if (r != null && r != value &&
                            type.IsInstanceOfType(r))
                        {
                            return r;
                        }
                        NSJSObject o = value as NSJSObject;
                        if (o == null)
                        {
                            return null;
                        }
                        return ToObject(constructor, o);
                    });
                    converter = Expression.GetFuncType(typeof(NSJSValue), type).GetConstructors()[0].Invoke(new[] { FV2O.Target,
                        FV2O.Method.MethodHandle.GetFunctionPointer() });
                    miToValueTable.Add(type, converter);
                }
                if (!miToValueConverterTable.ContainsKey(type))
                {
                    ParameterExpression p_v = Expression.Parameter(typeof(NSJSValue));
                    Func<NSJSValue, object> f_c = Expression.Lambda<Func<NSJSValue, object>>(Expression.Convert(Expression.Call(
                        Expression.Constant(converter), "Invoke", null, new[] { p_v }), typeof(object)), p_v).Compile();
                    miToValueConverterTable.Add(type, f_c);
                }
                InternalCheckKeyMembers(type);
                InternalCheckNetToObjectCallables(type);
                if (mode == 1)
                {
                    Func<NSJSValue, object> f_c;
                    miToValueConverterTable.TryGetValue(type, out f_c);
                    return f_c;
                }
                if (mode == 2)
                {
                    IDictionary<string, MemberInfo> d_mi;
                    miToValueKeyMemberTable.TryGetValue(type, out d_mi);
                    return d_mi;
                }
            }
            return converter;
        }

        private static IDictionary<string, MemberInfo> InternalCheckKeyMembers(Type type)
        {
            IDictionary<string, MemberInfo> d;
            lock (syncobj)
            {
                if (miToValueKeyMemberTable.TryGetValue(type, out d))
                {
                    return d;
                }
                d = new Dictionary<string, MemberInfo>();
                Func<MemberInfo, bool> predicate = i =>
                {
                    if (!d.ContainsKey(i.Name))
                    {
                        d.Add(i.Name, i);
                    }
                    return false;
                };
                type.GetProperties().FirstOrDefault(predicate);
                type.GetFields().FirstOrDefault(predicate);
                miToValueKeyMemberTable.Add(type, d);
            }
            return d;
        }

        private static object ToObject(ConstructorInfo constructor, NSJSObject obj)
        {
            if (constructor == null || obj == null)
            {
                return null;
            }
            object o = constructor.Invoke(null);
            IDictionary<string, MemberInfo> members = InternalCheckKeyMembers(constructor.DeclaringType);
            foreach (string key in obj.GetAllKeys())
            {
                MemberInfo mi;
                if (!members.TryGetValue(key, out mi))
                {
                    continue;
                }
                PropertyInfo pi = mi as PropertyInfo;
                FieldInfo fi = mi as FieldInfo;
                Type mt = pi != null ? pi.PropertyType : fi.FieldType;

                Func<NSJSValue, object> converter = (Func<NSJSValue, object>)InternalCheckConverter(mt, 1);
                if (converter == null)
                {
                    throw new InvalidOperationException(string.Format("Unable to find a valid JavaScript value To {0} type converter", mt.FullName));
                }
                object value = converter(obj.Get(mi.Name));
                if (pi == null)
                {
                    fi.SetValue(o, value);
                }
                else
                {
                    pi.SetValue(o, value, null);
                }
            }
            return o;
        }

        private static Expression ComplierCalleeDelegateTarget(MethodInfo m, Expression self, ParameterExpression arguments, IList<ParameterExpression> args)
        {
            IList<ParameterExpression> pushs = args;
            if (m.GetParameters().Length != pushs.Count)
            {
                pushs = new List<ParameterExpression>(args);
                pushs.Insert(0, arguments);
            }
            Expression expression = Expression.Call(self, m, pushs);
            if (m.ReturnType != typeof(void))
            {
                MethodInfo miSetReturnValue = typeof(NSJSFunctionCallbackInfo).GetMethod("SetReturnValue", new[] { m.ReturnType });
                if (miSetReturnValue != null)
                {
                    expression = Expression.Call(self, miSetReturnValue, expression);
                }
                else
                {
                    MethodInfo miToValue = typeof(SimpleAgent).GetMethod("ToValue", BindingFlags.NonPublic | BindingFlags.Static, null,
                        new[] { typeof(NSJSFunctionCallbackInfo), typeof(object) }, null).MakeGenericMethod(m.ReturnType);
                    expression = Expression.Call(miToValue, arguments, expression);
                    expression = Expression.Call(arguments, typeof(NSJSFunctionCallbackInfo).GetMethod("SetReturnValue", new[] { typeof(NSJSValue) }), expression);
                }
            }
            return expression;
        }

        protected internal static NSJSValue ToValue<T>(NSJSFunctionCallbackInfo arguemnts, object value)
        {
            return ToObject(arguemnts.VirtualMachine, value);
        }

        private class NetToObjectCallables
        {
            public Type disposable;
            public ISet<MethodInfo> callables;
        }

        private static NetToObjectCallables InternalCheckNetToObjectCallables(Type owner)
        {
            lock (syncobj)
            {
                NetToObjectCallables callables;
                if (!netToObjectCallablesTable.TryGetValue(owner, out callables))
                {
                    ISet<MethodInfo> props = new HashSet<MethodInfo>();
                    foreach (PropertyInfo pi in owner.GetProperties())
                    {
                        MethodInfo mi = pi.GetGetMethod();
                        if (mi != null)
                        {
                            props.Add(mi);
                        }
                        mi = pi.GetSetMethod();
                        if (mi != null)
                        {
                            props.Add(mi);
                        }
                    }
                    Type disposable = owner.GetInterface(typeof(System.IDisposable).FullName);
                    ISet<MethodInfo> methods = new HashSet<MethodInfo>();
                    foreach (MethodInfo m in owner.GetMethods())
                    {
                        if (m.DeclaringType == typeof(object))
                        {
                            continue;
                        }
                        if (disposable != null && m.Name == "Dispose")
                        {
                            continue;
                        }
                        if (props.Contains(m))
                        {
                            continue;
                        }
                        methods.Add(m);
                    }
                    callables = new NetToObjectCallables();
                    if (methods.Count > 0)
                    {
                        callables.callables = methods;
                    }
                    callables.disposable = disposable;
                    netToObjectCallablesTable.Add(owner, callables);
                }
                if (callables.callables == null && callables.disposable == null)
                {
                    return null;
                }
                return callables;
            }
        }

        protected internal static NSJSValue ToObject(NSJSVirtualMachine machine, object obj)
        {
            if (machine == null)
            {
                return null;
            }
            if (obj == null)
            {
                return NSJSValue.Null(machine);
            }
            Type owner = obj.GetType();
            NSJSObject objective = NSJSObject.New(machine);
            foreach (MemberInfo mi in InternalCheckKeyMembers(owner).Values)
            {
                PropertyInfo pi = mi as PropertyInfo;
                FieldInfo fi = mi as FieldInfo;
                object value = null;
                Type clazz = null;
                string key = mi.Name;
                if (pi != null)
                {
                    clazz = pi.PropertyType;
                    value = pi.GetValue(obj, null);
                }
                else
                {
                    clazz = fi.FieldType;
                    value = fi.GetValue(obj);
                }
                NSJSValue result = null;
                do
                {
                    if (value == null)
                    {
                        break;
                    }
                    Type element = TypeTool.GetArrayElement(clazz);
                    if (element == null && value is IList)
                    {
                        result = ArrayAuxiliary.ToArray(machine, element, (IList)value);
                    }
                    else if (TypeTool.IsBasicType(clazz) && !TypeTool.IsIPAddress(clazz))
                    {
                        result = value.As(machine);
                    }
                    else
                    {
                        result = ToObject(machine, value);
                    }
                } while (false);
                if (result == null)
                {
                    result = NSJSValue.Null(machine);
                }
                objective.Set(key, result);
            }
            NetToObjectCallables callables = InternalCheckNetToObjectCallables(owner);
            if (callables != null)
            {
                foreach (MethodInfo m in callables.callables)
                {
                    objective.Set(m.Name, NSJSPinnedCollection.Pinned(Complier(m)));
                }
                objective.Set("Dispose", FDEFAULTDISPOSE);
                if (!objective.IsDefined("Close"))
                {
                    objective.Set("Close", FDEFAULTDISPOSE);
                }
                NSJSKeyValueCollection.Set(objective, obj);
            }
            return objective;
        }

        private static NSJSFunctionCallback GetCompilerCache(MethodInfo m)
        {
            CompilerCacheKey key = new CompilerCacheKey
            {
                method = m,
                type = m.DeclaringType,
            };
            lock (syncobj)
            {
                NSJSFunctionCallback callback;
                fnCompilerCaches.TryGetValue(key, out callback);
                return callback;
            }
        }

        private static IList<ParameterExpression> GetLocalVar(MethodInfo m)
        {
            IList<ParameterExpression> localvar = new List<ParameterExpression>();
            ParameterInfo[] s = m.GetParameters();
            for (int i = 0; i < s.Length; i++)
            {
                ParameterInfo p = s[i];
                if (i <= 0 && typeof(NSJSFunctionCallbackInfo).IsAssignableFrom(p.ParameterType))
                {
                    continue;
                }
                localvar.Add(Expression.Variable(p.ParameterType, p.Name));
            }
            return localvar;
        }

        public static NSJSFunctionCallback Complier(MethodInfo m)
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }
            CompilerCacheKey key = new CompilerCacheKey
            {
                method = m,
                type = m.DeclaringType,
            };
            lock (syncobj)
            {
                NSJSFunctionCallback callback;
                if (fnCompilerCaches.TryGetValue(key, out callback))
                {
                    return callback;
                }
                else
                {
                    ParameterExpression arguments = Expression.Parameter(typeof(NSJSFunctionCallbackInfo), "arguments");
                    ParameterExpression self = Expression.Variable(m.DeclaringType, "this");

                    IList<ParameterExpression> localvar = GetLocalVar(m);
                    Expression<Action<NSJSFunctionCallbackInfo>> expression = Expression.Lambda<Action<NSJSFunctionCallbackInfo>>(
                        Expression.Block(
                            new ParameterExpression[] { self },
                            new Expression[]
                            {
                               Expression.Block(
                                    localvar,
                                    ComplierBlock(m, arguments, self, localvar)
                                )
                            }
                        )
                    , arguments);

                    ParameterExpression info = Expression.Parameter(typeof(IntPtr), "info");
                    Expression<NSJSFunctionCallback> launcher = Expression.Lambda<NSJSFunctionCallback>(
                        Expression.Block(new ParameterExpression[] { arguments },
                            Expression.Assign(arguments, Expression.Call(Get<MethodInfo>(typeof(NSJSFunctionCallbackInfo), "From"), info)),
                            Expression.Call(Expression.Constant(JSINVKHANDLING), "Invoke", null, arguments, Expression.Constant(expression.Compile()))
                        ), info);

                    callback = launcher.Compile();
                    fnCompilerCaches.Add(key, callback);
                    return callback;
                }
            }
        }

        public static Func<NSJSValue, TResult> GetConverter<TResult>()
        {
            return (Func<NSJSValue, TResult>)InternalCheckConverter(typeof(TResult));
        }

        public static Func<NSJSValue, object> GetConverterBox(Type type)
        {
            if (type == null)
            {
                return null;
            }
            return (Func<NSJSValue, object>)InternalCheckConverter(type, 1);
        }

        public static IDictionary<Type, Delegate> GetAllConverter()
        {
            IDictionary<Type, Delegate> d = new Dictionary<Type, Delegate>();
            lock (syncobj)
            {
                miToValueTable.FirstOrDefault(kv =>
                {
                    d.Add(kv.Key, (Delegate)kv.Value);
                    return false;
                });
            }
            return d;
        }

        public static void SetConverter<TResult>(Func<NSJSValue, TResult> converter)
        {
            lock (syncobj)
            {
                Type type = typeof(TResult);
                if (converter == null)
                {
                    miToValueTable.Remove(type);
                    miToValueKeyMemberTable.Remove(type);
                    miToValueConverterTable.Remove(type);
                }
                else
                {
                    object previous = null;
                    if (!miToValueTable.TryGetValue(type, out previous))
                    {
                        miToValueTable.Add(type, converter);
                        previous = InternalCheckConverter(type);
                    }
                    else if (!object.Equals(previous, converter))
                    {
                        miToValueTable[type] = converter;
                        miToValueKeyMemberTable.Remove(type);
                        miToValueConverterTable.Remove(type);
                        previous = InternalCheckConverter(type);
                    }
                }
            }
        }
    }
}
