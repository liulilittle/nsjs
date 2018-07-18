namespace nsjsdotnet.Core
{
    using nsjsdotnet;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Mail;
    using System.Reflection;

    public class SimpleAgent
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, MemberInfo> miTable = new Dictionary<string, MemberInfo>()
        {
            { K(typeof(NSJSFunctionCallbackInfo), "From"), typeof(NSJSFunctionCallbackInfo).GetMethod("From", new[] { typeof(IntPtr) }) },
            { K(typeof(NSJSKeyValueCollection), "Get"), typeof(NSJSKeyValueCollection).GetMethods().First(m => !m.IsGenericMethod && m.Name == "Get" && m.GetParameters().FirstOrDefault(i => i.ParameterType == typeof(NSJSObject)) != null) },
        };

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

        private TResult Get<TType, TResult>(string name) where TResult : MemberInfo
        {
            return Get<TResult>(typeof(TType), name);
        }

        private TResult Get<TResult>(Type type, string name) where TResult : MemberInfo
        {
            string key = K(type, name);
            return miTable[key] as TResult;
        }

        public SimpleAgent()
        {
            miToValueTable.Keys.FirstOrDefault(i => InternalConverter(i) == null);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<Type, object> miToValueTable = new Dictionary<Type, object>()
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
        private readonly IDictionary<Type, Func<NSJSValue, object>> miToValueConverterTable = new Dictionary<Type, Func<NSJSValue, object>>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<Type, Dictionary<string, MemberInfo>> miToValueKeyMemberTable = new Dictionary<Type, Dictionary<string, MemberInfo>>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<CompilerCacheKey, NSJSFunctionCallback> fnCompilerCaches = new Dictionary<CompilerCacheKey, NSJSFunctionCallback>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Func<NSJSFunctionCallbackInfo, int, NSJSValue> FCIGTITEM = (arguments, index) =>
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
        private readonly Action<NSJSFunctionCallbackInfo, Action<NSJSFunctionCallbackInfo>> JSINVKHANDLING = (arguments, e) =>
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

        private Expression Throw(Exception exception)
        {
            return Throw(Expression.Constant(exception));
        }

        private Expression Throw(Expression exception)
        {
            return Expression.Throw(exception);
        }

        private IEnumerable<Expression> ComplierBlock(MethodInfo m, ParameterExpression arguments, ParameterExpression self,
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

                    Expression invk = Expression.Call(Expression.Constant(InternalConverter(type)), "Invoke",
                        null, new Expression[] { get });
                    expressions.Add(Expression.Assign(key, invk));
                }
            }
            expressions.Add(ComplierCalleeDelegateTarget(m, self, arguments, localvar));
            expressions.Add(Expression.Constant(null));
            return expressions;
        }

        private object InternalConverter(Type type, int mode = 0)
        {
            object converter = null;
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
                    return ConverterToObject(constructor, o);
                });
                converter = Expression.GetFuncType(typeof(NSJSValue), type).GetConstructors()[0].Invoke(new[] { FV2O.Target, FV2O.Method.MethodHandle.GetFunctionPointer() });
                miToValueTable.Add(type, converter);
            }
            if (!miToValueConverterTable.ContainsKey(type))
            {
                ParameterExpression p_v = Expression.Parameter(typeof(NSJSValue));
                Func<NSJSValue, object> f_c = Expression.Lambda<Func<NSJSValue, object>>(Expression.Convert(Expression.Call(
                    Expression.Constant(converter), "Invoke", null, new[] { p_v }), typeof(object)), p_v).Compile();
                miToValueConverterTable.Add(type, f_c);
            }
            if (!miToValueKeyMemberTable.ContainsKey(type))
            {
                Dictionary<string, MemberInfo> d;
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
            if (mode == 1)
            {
                Func<NSJSValue, object> f_c;
                miToValueConverterTable.TryGetValue(type, out f_c);
                return f_c;
            }
            if (mode == 2)
            {
                Dictionary<string, MemberInfo> d_mi;
                miToValueKeyMemberTable.TryGetValue(type, out d_mi);
                return d_mi;
            }
            return converter;
        }

        private object ConverterToObject(ConstructorInfo constructor, NSJSObject obj)
        {
            if (constructor == null || obj == null)
            {
                return null;
            }
            object o = constructor.Invoke(null);
            IDictionary<string, MemberInfo> members = (IDictionary<string, MemberInfo>)InternalConverter(constructor.DeclaringType, 2);
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

                Func<NSJSValue, object> converter = (Func<NSJSValue, object>)InternalConverter(mt, 1);
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

        private Expression ComplierCalleeDelegateTarget(MethodInfo m, Expression self, ParameterExpression arguments, IList<ParameterExpression> args)
        {
            IList<ParameterExpression> pushs = args;
            if (m.GetParameters().Length != pushs.Count)
            {
                pushs = new List<ParameterExpression>(args);
                pushs.Insert(0, arguments);
            }
            Expression expression = Expression.Call(self, m, pushs);
            MethodInfo miSetReturnValue = typeof(NSJSFunctionCallbackInfo).GetMethod("SetReturnValue", new[] { m.ReturnType });
            if (miSetReturnValue != null)
            {
                expression = Expression.Call(self, miSetReturnValue, expression);
            }
            else
            {
                MethodInfo miToValue = typeof(SimpleAgent).GetMethod("ToValue", BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(NSJSFunctionCallbackInfo), typeof(object) }, null).MakeGenericMethod(m.ReturnType);
                expression = Expression.Call(Expression.Constant(this), miToValue, arguments, expression);
                expression = Expression.Call(arguments, typeof(NSJSFunctionCallbackInfo).GetMethod("SetReturnValue", new[] { typeof(NSJSValue) }), expression);
            }
            return expression;
        }

        protected virtual NSJSValue ToValue<T>(NSJSFunctionCallbackInfo arguemnts, object value)
        {
            return ObjectAuxiliary.ToObject(arguemnts.VirtualMachine, value);
        }

        private IList<ParameterExpression> GetLocalVar(MethodInfo m)
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

        public virtual NSJSFunctionCallback Complier(MethodInfo m)
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

        public virtual Func<NSJSValue, TResult> GetConverter<TResult>()
        {
            return (Func<NSJSValue, TResult>)InternalConverter(typeof(TResult));
        }

        public virtual Func<NSJSValue, object> GetConverterBox(Type type)
        {
            if (type == null)
            {
                return null;
            }
            return (Func<NSJSValue, object>)InternalConverter(type, 1);
        }

        public virtual IDictionary<Type, Delegate> GetAllConverter()
        {
            IDictionary<Type, Delegate> d = new Dictionary<Type, Delegate>();
            miToValueTable.FirstOrDefault(kv =>
            {
                d.Add(kv.Key, (Delegate)kv.Value);
                return false;
            });
            return d;
        }

        public virtual void SetConverter<TResult>(Func<NSJSValue, TResult> converter)
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
                    previous = InternalConverter(type);
                }
                else if (!object.Equals(previous, converter))
                {
                    miToValueTable[type] = converter;
                    miToValueKeyMemberTable.Remove(type);
                    miToValueConverterTable.Remove(type);
                    previous = InternalConverter(type);
                }
            }
        }
    }
}
