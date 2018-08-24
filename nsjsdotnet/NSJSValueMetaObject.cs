namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using Converter = System.Convert;

    public class NSJSValueMetaObject : DynamicMetaObject
    {
        private readonly Func<Type, string, object, object> f_SetValue = null;
        private readonly Func<Type, string, object> f_GetValue = null;
        private readonly Func<Type, object> f_Convert = null;

        public NSJSValueMetaObject(NSJSValue value, Expression expression) :
            base(expression, BindingRestrictions.Empty, value)
        {
            this.f_GetValue = this.GetValue;
            this.f_SetValue = (a1, a2, a3) =>
            {
                this.SetValue(a1, a2, a3);
                return this.Value;
            };
            this.f_Convert = this.Convert;
        }

        protected virtual object Convert(Type type)
        {
            return ConvertValue(type, this.Value as NSJSValue);
        }

        protected static internal object ConvertValue(Type type, NSJSValue value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object o = FetchValue(type, value);
            if (type == typeof(int))
            {
                o = (o == null ? 0 : Converter.ToInt32(o));
            }
            else if (type == typeof(uint))
            {
                o = (o == null ? 0u : Converter.ToUInt32(o));
            }
            else if (type == typeof(short))
            {
                o = (o == null ? (short)0 : Converter.ToInt16(o));
            }
            else if (type == typeof(ushort))
            {
                o = (o == null ? (ushort)0 : Converter.ToUInt16(o));
            }
            else if (type == typeof(sbyte))
            {
                o = (o == null ? (sbyte)0 : Converter.ToSByte(o));
            }
            else if (type == typeof(byte))
            {
                o = (o == null ? (byte)0 : Converter.ToByte(o));
            }
            else if (type == typeof(long))
            {
                o = (o == null ? 0L : Converter.ToInt64(o));
            }
            else if (type == typeof(ulong))
            {
                o = (o == null ? 0ul : Converter.ToUInt64(o));
            }
            else if (type == typeof(float))
            {
                o = (o == null ? 0f : Converter.ToSingle(o));
            }
            else if (type == typeof(double))
            {
                o = (o == null ? 0d : Converter.ToDouble(o));
            }
            else if (type == typeof(decimal))
            {
                o = (o == null ? 0m : Converter.ToDecimal(o));
            }
            else if (type == typeof(char))
            {
                o = (o == null ? '\0' : Converter.ToChar(o));
            }
            else if (type == typeof(DateTime))
            {
                long ticks = 0;
                if (o is long)
                {
                    ticks = (long)o;
                }
                else if (o != null)
                {
                    ticks = Converter.ToInt64(o);
                }
                o = NSJSDateTime.LocalDateToDateTime(ticks);
            }
            else if (type == typeof(string))
            {
                if (o == null)
                {
                    o = null;
                }
                else if (!(o is string))
                {
                    o = o.ToString();
                }
            }
            return o;
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            Expression expression = Expression.Convert(Expression.Call(Expression.Constant(this.f_Convert),
                "Invoke", null, new[] { Expression.Constant(binder.Type) }), binder.Type);
            return new DynamicMetaObject(expression,
                BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return new DynamicMetaObject(Expression.Call(Expression.Constant(this.f_GetValue),
                "Invoke", null,
                Expression.Constant(binder.ReturnType), Expression.Constant(binder.Name)),
                BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            DynamicMetaObject index = indexes.Length == 1 ? indexes[0] : null;
            if (index == null || index.LimitType != typeof(int))
            {
                return base.BindGetIndex(binder, indexes);
            }
            return new DynamicMetaObject(Expression.Call(Expression.Constant(this.f_GetValue),
                "Invoke", null,
                Expression.Constant(binder.ReturnType), Expression.Constant(index.Value.ToString())),
                BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            DynamicMetaObject index = indexes.Length == 1 ? indexes[0] : null;
            if (index == null || index.LimitType != typeof(int) || value == null)
            {
                return base.BindSetIndex(binder, indexes, value);
            }
            return new DynamicMetaObject(Expression.Call(Expression.Constant(this.f_SetValue),
                "Invoke", null,
                Expression.Constant(value.LimitType),
                Expression.Constant(index.Value.ToString()),
                Expression.Convert(Expression.Constant(value.Value), typeof(object))),
                BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return new DynamicMetaObject(Expression.Call(Expression.Constant(this.f_SetValue),
                "Invoke", null,
                Expression.Constant(value.LimitType),
                Expression.Constant(binder.Name),
                Expression.Convert(Expression.Constant(value.Value), typeof(object))),
                BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            NSJSObject owner = this.Value as NSJSObject;
            if (owner == null)
            {
                return new string[0];
            }
            return owner.GetPropertyNames();
        }

        public virtual object GetValue(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentException("key");
            }
            NSJSObject owner = this.Value as NSJSObject;
            NSJSValue value = null;
            if (owner != null)
            {
                value = owner.Get(key);
            }
            return this.GetValue(type, value);
        }

        protected virtual object GetValue(Type type, NSJSValue value)
        {
            return FetchValue(type, value);
        }

        protected internal static object FetchValue(Type type, NSJSValue value)
        {
            if (type == null || value == null || value.IsNullOrUndfined)
            {
                return null;
            }
            if ((value.DateType & NSJSDataType.kFunction) > 0)
            {
                return value;
            }
            if ((value.DateType & NSJSDataType.kArray) > 0 ||
                (value.DateType & NSJSDataType.kObject) > 0)
            {
                Func<NSJSValue, object> converter = SimpleAgent.GetConverterBox(type);
                return converter(value);
            }
            return value.GetValue();
        }

        public virtual void SetValue(Type type, string key, object value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.Length <= 0)
            {
                throw new ArgumentException("key");
            }
            NSJSObject owner = this.Value as NSJSObject;
            do
            {
                if (owner == null)
                {
                    break;
                }
                if (value == null)
                {
                    owner.Set(key, NSJSValue.Null(owner.VirtualMachine));
                    break;
                }
                NSJSValue v = ValueAuxiliary.As(value, owner.VirtualMachine);
                if (!v.IsNullOrUndfined)
                {
                    owner.Set(key, v);
                    break;
                }
                owner.Set(key, SimpleAgent.ToObject(owner.VirtualMachine, value));
            } while (false);
        }
    }
}
