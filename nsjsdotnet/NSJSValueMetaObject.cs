namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;

    public class NSJSValueMetaObject : DynamicMetaObject
    {
        private readonly Func<Type, string, object, object> f_SetValue = null;
        private readonly Func<Type, string, object> f_GetValue = null;

        public NSJSValueMetaObject(NSJSValue value, Expression expression) :
            base(expression, BindingRestrictions.Empty, value)
        {
            this.f_GetValue = this.GetValue;
            this.f_SetValue = (a1, a2, a3) =>
            {
                this.SetValue(a1, a2, a3);
                return this.Value;
            };
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
            if (value == null || value.IsNullOrUndfined)
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
