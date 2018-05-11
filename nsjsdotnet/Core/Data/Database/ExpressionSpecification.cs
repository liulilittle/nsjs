namespace nsjsdotnet.Core.Data.Database
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ExpressionSpecification
    {
        public static string SatisfiedBy<T>(Expression<Func<T, bool>> express, string membername) // 规约
        {
            return ResolveExpression(express.Body, express.Parameters, membername);
        }

        public static string SatisfiedBy<T, E>(Expression<Func<T, E, bool>> express, string membername)
        {
            return ResolveExpression(express.Body, express.Parameters, membername);
        }

        public static string SatisfiedBy<T>(Expression<Func<T, object>> express, string membername)
        {
            return ResolveExpression(express.Body, express.Parameters, membername);
        }

        private static string ResolveBinaryExpression(Expression express, string symbol, ReadOnlyCollection<ParameterExpression> key, string membername)
        {
            if (express is BinaryExpression)
            {
                BinaryExpression e = (BinaryExpression)express;
                // 
                string left = ResolveExpression(e.Left, key, membername);
                string right = ResolveExpression(e.Right, key, membername);
                //
                return string.Format("{0} {1} {2}", left, symbol, right);
            }
            return null;
        }

        private static string ResolveConvertExpression(Expression express, ReadOnlyCollection<ParameterExpression> key, string membername)
        {
            if (express is UnaryExpression)
            {
                UnaryExpression e = (UnaryExpression)express;
                //
                return ResolveExpression(e.Operand, key, membername);
            }
            return null;
        }

        private static string ResolveNotAndEqualsExpress(Expression express, ReadOnlyCollection<ParameterExpression> key, string membername)
        {
            if (express is BinaryExpression)
            {
                BinaryExpression e = (BinaryExpression)express;
                //
                string left = ResolveExpression(e.Left, key, membername);
                string right = ResolveExpression(e.Right, key, membername);
                //
                Type clazz = e.Type;
                //
                if (left != null && right != null)
                {
                    if (e.NodeType == ExpressionType.NotEqual)
                    {
                        return string.Format("{0} != {1}", left, right);
                    }
                    return string.Format("{0} = {1}", left, right);
                }
                else
                {
                    string value = left != null ? left : right;
                    if (e.NodeType == ExpressionType.NotEqual)
                    {
                        return string.Format("{0} IS NOT NULL", value);
                    }
                    return string.Format("{0} IS NULL", value);
                }
            }
            return null;
        }

        private static object ResolveMemberResource(object obj, MemberInfo mi, string membername)
        {
            if (obj == null || mi == null)
                return null;
            object value = null;
            switch (mi.MemberType)
            {
                case MemberTypes.Property:
                    value = ((PropertyInfo)mi).GetValue(obj, null);
                    break;
                case MemberTypes.Field:
                    value = ((FieldInfo)mi).GetValue(obj);
                    break;
            }
            return value;
        }

        private static string ResolveMemberExpression(Expression express, ReadOnlyCollection<ParameterExpression> key, string membername)
        {
            if (express is MemberExpression)
            {
                MemberExpression e = (MemberExpression)express;
                MemberInfo mi = e.Member;
                // 
                if (!key.Contains(e.Expression as ParameterExpression))
                {
                    object value = value = ResolveMemberResource(ResolveResourceExpress(e.Expression, membername)
                        , mi, membername); 
                    Type clazz = e.Type;
                    //
                    if (clazz == typeof(string) || clazz == typeof(DateTime) || 
                        (clazz.IsGenericType && clazz.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        return string.Format("'{0}'", value);
                    }
                    else
                    {
                        return string.Format("{0}", value);
                    }
                }
                ParameterExpression pi = (ParameterExpression)e.Expression;
                return string.Format("[{0}].[{1}]", (string.IsNullOrEmpty(membername) ? pi.Name : membername), mi.Name);
            }
            return null;
        }

        private static string ResolveConstantExpression(Expression express, ReadOnlyCollection<ParameterExpression> key, string membername)
        {
            if (express is ConstantExpression)
            {
                ConstantExpression e = (ConstantExpression)express;
                //
                Type clazz = e.Type;
                //
                if (clazz == typeof(string) || clazz == typeof(DateTime) || 
                    (clazz.IsGenericType && clazz.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    if (e.Value == null)
                    {
                        return null;
                    }
                    return string.Format("'{0}'", e.Value);
                }
                else
                {
                    if (clazz == typeof(bool))
                        return ((bool)e.Value == true ? "1" : "0");
                    return string.Format("{0}", e.Value);
                }
            }
            return null;
        }

        private static string ResolveExpression(Expression e, ReadOnlyCollection<ParameterExpression> key, string membername)
        {
            string express = null;
            object value = null;
            //
            if (e.NodeType == ExpressionType.And || e.NodeType == ExpressionType.AndAlso)
            {
                value = ResolveBinaryExpression(e, "AND", key, membername);
            }
            else if (e.NodeType == ExpressionType.NotEqual)
            {
                value = ResolveNotAndEqualsExpress(e, key, membername);
            }
            else if (e.NodeType == ExpressionType.Equal)
            {
                value = ResolveNotAndEqualsExpress(e, key, membername);
            }
            else if (e.NodeType == ExpressionType.GreaterThan)
            {
                value = ResolveBinaryExpression(e, ">", key, membername);
            }
            else if (e.NodeType == ExpressionType.Or || e.NodeType == ExpressionType.OrElse)
            {
                value = ResolveBinaryExpression(e, "OR", key, membername);
            }
            else if (e.NodeType == ExpressionType.LessThan)
            {
                value = ResolveBinaryExpression(e, "<", key, membername);
            }
            else if (e.NodeType == ExpressionType.Convert)
            {
                value = ResolveConvertExpression(e, key, membername);
            }
            else if (e.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                value = ResolveBinaryExpression(e, ">=", key, membername);
            }
            else if (e.NodeType == ExpressionType.LessThanOrEqual)
            {
                value = ResolveBinaryExpression(e, "<=", key, membername);
            }
            else if (e.NodeType == ExpressionType.Constant)
            {
                value = ResolveConstantExpression(e, key, membername);
            }
            else if (e.NodeType == ExpressionType.MemberAccess)
            {
                value = ResolveMemberExpression(e, key, membername);
            }
            else if (e.NodeType == ExpressionType.Call)
            {
                value = ResolveCallExpression(e, membername);
            }
            if (value != null)
            {
                express += value;
            }
            return express;
        }

        private static bool IsAnonymousType(Type clazz)
        {
            string name = clazz.Name;
            return name.Contains("<>");
        }

        private static object ResolveResourceExpress(Expression express, string membername)
        {
            if (express != null)
            {
                if (express.NodeType == ExpressionType.Constant)
                {
                    ConstantExpression cs = (ConstantExpression)express;
                    //
                    Type clazz = cs.Type;
                    object value = cs.Value;

                    return value;
                }
                else if (express.NodeType == ExpressionType.MemberAccess)
                {
                    return ResolveResourceExpress(((MemberExpression)express).Expression, membername);
                }
                else if (express.NodeType == ExpressionType.Call)
                {
                    MethodCallExpression call = (MethodCallExpression)express;
                    //
                    MethodInfo met = call.Method;
                    var args = call.Arguments;
                    //
                    object[] values = new object[args.Count];
                    //
                    for (int i = 0; i < args.Count; i++)
                    {
                        values[i] = ResolveResourceExpress(args[i], membername);
                    }
                    return met.Invoke(call.Object, values);
                }
            }
            return null;
        }

        private static string ResolveCallExpression(Expression express, string membername)
        {
            if (express is MethodCallExpression)
            {
                MethodCallExpression e = (MethodCallExpression)express;
                //
                return Convert.ToString(ResolveResourceExpress(e, membername));
            }
            return null;
        }
    }
}
