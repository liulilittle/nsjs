namespace nsjsdotnet.Core.Data.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;

    public sealed class ExpandoObject : IDynamicMetaObjectProvider, IEnumerable<string>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IDictionary<string, object> _dict = new Dictionary<string, object>();
        private class ExpandoMetaObject : DynamicMetaObject
        {
            private static readonly MethodInfo set = typeof(ExpandoMetaObject).GetMethod("Set", BindingFlags.NonPublic |
        BindingFlags.Instance);
            private static readonly MethodInfo get = typeof(ExpandoMetaObject).GetMethod("Get", BindingFlags.NonPublic |
        BindingFlags.Instance);
            private static readonly MethodInfo invoke = typeof(ExpandoMetaObject).GetMethod("Invoke", BindingFlags.NonPublic |
        BindingFlags.Instance);
            private IDictionary<string, object> dict = null;
            public ExpandoMetaObject(ExpandoObject expando, Expression express)
        : base(express, BindingRestrictions.Empty, expando)
            {
                dict = expando._dict;
            }
            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                return InvokeMember(binder.Name, get);
            }
            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                return InvokeMember(binder.Name, set, value.Value);
            }
            private DynamicMetaObject InvokeMember(string key, MethodInfo met, params object[] values)
            {
                object args = null; if (met == invoke) args = values; else if (values != null && values.Length > 0) args = values[0];
                return new DynamicMetaObject(Expression.Call(Expression.Constant(this), met,
                    Expression.Constant(key, typeof(string)),
                    Expression.Convert(Expression.Constant(args), typeof(object))),
                    BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
            }
            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                object[] s = new object[args.Length];
                for (int i = 0; i < s.Length; i++)
                {
                    s[i] = args[i].Value;
                }
                return InvokeMember(binder.Name, invoke, s);
            }
            private object Invoke(string key, object values)
            {
                object obj = null;
                lock (this)
                {
                    if (dict.ContainsKey(key))
                    {
                        obj = dict[key];
                    }
                }
                Delegate d = obj as Delegate;
                if (d == null)
                {
                    throw new MethodAccessException();
                }
                return d.DynamicInvoke(values as object[]);
            }
            public override IEnumerable<string> GetDynamicMemberNames()
            {
                lock (dict)
                {
                    return dict.Keys;
                }
            }
            private object Get(string key, object value)
            {
                lock (dict)
                {
                    if (!dict.ContainsKey(key))
                    {
                        throw new MemberAccessException(key);
                    }
                    return dict[key];
                }
            }
            private object Set(string key, object value)
            {
                lock (dict)
                {
                    if (dict.ContainsKey(key))
                    {
                        dict[key] = value;
                    }
                    else
                    {
                        dict.Add(key, value);
                    }
                }
                return null;
            }
        }
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression express)
        {
            return new ExpandoMetaObject(this, express);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (string key in this)
            {
                yield return key;
            }
        }
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            lock (_dict)
            {
                var keys = _dict.Keys;
                return keys.GetEnumerator();
            }
        }
    }
}
