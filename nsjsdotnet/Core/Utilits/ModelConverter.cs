namespace nsjsdotnet.Core.Utilits
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using nsjsdotnet.Core.Linq;

    public static class ModelConverter
    {
        private static readonly IDictionary<Type, IDictionary<Type, Func<IList, IList, IList>>> m_pRegTListCopys = 
            new Dictionary<Type, IDictionary<Type, Func<IList, IList, IList>>>();
        private static readonly IDictionary<Type, IDictionary<Type, Func<object, object>>> m_pRegTObjCopys = 
            new Dictionary<Type, IDictionary<Type, Func<object, object>>>();
        private static readonly object m_pLook = new object();

        /// <summary>
        /// 注册列表拷贝（快速）
        /// </summary>
        public static bool RegisterCopyList(Type desc, Type src)
        {
            if (src == null || desc == null)
            {
                return false;
            }
            lock (m_pLook)
            {
                IDictionary<Type, Func<IList, IList, IList>> l_pLstConvertTo = null;
                if (!m_pRegTListCopys.TryGetValue(src, out l_pLstConvertTo))
                {
                    l_pLstConvertTo = new Dictionary<Type, Func<IList, IList, IList>> ();
                    m_pRegTListCopys.Add(src, l_pLstConvertTo);
                }
                else if (l_pLstConvertTo.ContainsKey(desc))
                {
                    return false;
                }
                Func<IList, IList, IList> f = CreateCopyList(desc, src);
                l_pLstConvertTo.Add(desc, f);
            }
            return true;
        }

        /// <summary>
        /// 注册列表拷贝（快速）
        /// </summary>
        public static bool RegisterCopyList<TDesc, TSrc>()
        {
            return RegisterCopyList(typeof(TDesc), typeof(TSrc));
        }

        /// <summary>
        /// 注册类型拷贝（快速）
        /// </summary>
        public static bool RegisterCopy<TDesc, TSrc>()
        {
            return RegisterCopy(typeof(TDesc), typeof(TSrc));
        }

        /// <summary>
        /// 注册类型拷贝（快速）
        /// </summary>
        public static bool RegisterCopy(Type desc, Type src)
        {
            if (src == null || desc == null)
            {
                return false;
            }
            lock (m_pLook)
            {
                IDictionary<Type, Func<object, object>> l_pObjConvertTo = null;
                if (!m_pRegTObjCopys.TryGetValue(src, out l_pObjConvertTo))
                {
                    l_pObjConvertTo = new Dictionary<Type, Func<object, object>>();
                    m_pRegTObjCopys.Add(src, l_pObjConvertTo);
                }
                else if (l_pObjConvertTo.ContainsKey(desc))
                {
                    return false;
                }
                ParameterExpression p__ebp = Expression.Parameter(typeof(object));
                ParameterExpression p__desc = Expression.Variable(desc, "desc");
                ParameterExpression p__src = Expression.Variable(src, "src");
                List<Expression> ls = new List<Expression>() { Expression.Assign(p__src, Expression.Convert(p__ebp, src)), Expression.Assign(p__desc, Expression.New(desc)) };
                foreach (PropertyInfo pi in src.GetProperties())
                {
                    PropertyInfo pp = desc.GetProperty(pi.Name);
                    if (pp != null)
                    {
                        Type clazz = pp.PropertyType; // 编织
                        if ((clazz == pi.PropertyType) || TypeTool.IsBasicType(clazz) || TypeTool.IsBasicType(TypeTool.GetArrayElement(clazz)))
                        {
                            ls.Add(Expression.Assign(Expression.Property(p__desc, pp),
                                Expression.Convert(Expression.Property(p__src, pi), pp.PropertyType)));
                        }
                        else
                        {
                            if (!TypeTool.IsList(clazz))
                            {
                                ls.Add(Expression.IfThen(Expression.NotEqual(Expression.Property(p__src, pi), Expression.Constant(null)),
                                          Expression.Assign(
                                              Expression.Property(p__desc, pp),
                                              Expression.TypeAs(Expression.Call(
                                                  Expression.MakeIndex(
                                                      Expression.Constant(l_pObjConvertTo),
                                                      typeof(IDictionary<Type, Func<object, object>>).GetProperties()[0], new[] { Expression.Constant(clazz) }),
                                                      typeof(Func<object, object>).GetMethod("Invoke"), Expression.Property(p__src, pi)
                                                  ), clazz)
                                              )
                                          )
                                      );
                            }
                            else
                            {
                                Type xe_desc = TypeTool.GetArrayElement(clazz);
                                Type xe_src = pi.PropertyType;
                                ParameterExpression size = Expression.Variable(typeof(int), "size");
                                ParameterExpression j = Expression.Variable(typeof(int), "j");
                                ParameterExpression d = Expression.Variable(typeof(Func<object, object>), "d");
                                ParameterExpression m = Expression.Variable(clazz, "m");
                                ParameterExpression item = Expression.Variable(xe_desc, "item");

                                Expression get = Expression.Assign(item, Expression.TypeAs(Expression.Call(d, typeof(Func<object, object>).GetMethod("Invoke"), Expression.MakeIndex(Expression.Property(p__src, pi), xe_src.GetProperty("Item", new[] { typeof(int) }), new[] { Expression.PostIncrementAssign(j) })), xe_desc));
                                Expression set = null;
                                if (clazz.IsArray)
                                {
                                    set = Expression.MakeIndex(m, clazz.GetProperty("Item"), new[] { j });
                                    set = Expression.Assign(item, set);
                                }
                                else
                                {
                                    set = Expression.Call(m, clazz.GetMethod("Add"), new[] { item });
                                }
                                LabelTarget label = Expression.Label(typeof(int));
                                Expression exp = Expression.IfThen(Expression.NotEqual(Expression.Constant(null), Expression.Property(p__src, pi)),
                                    Expression.Block(new[] { size, j, d, m, item },
                                        Expression.Assign(size, Expression.Property(Expression.Property(p__src, pi), "Length")),
                                        Expression.Assign(j, Expression.Constant(0)),
                                        Expression.Assign(d,
                                                Expression.MakeIndex(
                                                    Expression.Constant(l_pObjConvertTo),
                                                    typeof(IDictionary<Type, Func<object, object>>).GetProperties()[0], new[] { Expression.Constant(xe_desc) })
                                                ),
                                        Expression.Assign(Expression.Property(p__desc, pp), Expression.New(clazz.GetConstructor(new Type[] { typeof(int) }), size)),
                                        Expression.Assign(m, Expression.Property(p__desc, pp)),
                                        Expression.Loop(Expression.IfThenElse(Expression.GreaterThanOrEqual(j, size),
                                                            Expression.Break(label, j),
                                                            Expression.Block(get, set))
                                        , label)
                                    )
                                );
                                ls.Add(exp);
                            }
                        }
                    }
                }
                Expression<Func<object, object>> ee = Expression.Lambda<Func<object, object>>(Expression.Block(new[] { p__desc, p__src },
                    Expression.Block(ls.ToArray()), p__desc), p__ebp);
                Func<object, object> f = ee.Compile();

                l_pObjConvertTo.Add(desc, f);

                return true;
            }
        }

        private static IDictionary<Type, Func<object, object>> GetObjectConvertTo(Type desc, Type src)
        {
            RegisterCopy(desc, src);
            IDictionary<Type, Func<object, object>> convertto = null;
            if (!m_pRegTObjCopys.TryGetValue(src, out convertto))
            {
                throw new KeyNotFoundException();
            }
            return convertto;
        }

        private static Func<IList, IList, IList> CreateCopyList(Type desc, Type src)
        {
            ParameterExpression s = Expression.Parameter(typeof(IList), "s");
            ParameterExpression n = Expression.Parameter(typeof(IList), "n");

            LabelTarget @continue = Expression.Label(typeof(void), "LABEL_0002");
            LabelTarget @break = Expression.Label(typeof(int), "LABEL_0001");

            ParameterExpression len = Expression.Variable(typeof(int), "len");
            ParameterExpression i = Expression.Variable(typeof(int), "i");

            ParameterExpression p__src = Expression.Variable(src, "src");
            ParameterExpression p__desc = Expression.Variable(desc, "desc");

            List<Expression> ls = new List<Expression>();
            ls.Add(Expression.Assign(p__desc, Expression.New(p__desc.Type)));
            foreach (PropertyInfo pi in src.GetProperties())
            {
                PropertyInfo pp = desc.GetProperty(pi.Name);
                if (pp != null)
                {
                    Type clazz = pp.PropertyType; // 编织
                    if ((clazz == pi.PropertyType) || TypeTool.IsBasicType(clazz) || TypeTool.IsBasicType(TypeTool.GetArrayElement(clazz)))
                    {
                        ls.Add(Expression.Assign(Expression.Property(p__desc, pp), Expression.Property(p__src, pi)));
                    }
                    else
                    {
                        if (!TypeTool.IsList(clazz))
                        {
                            ls.Add(Expression.IfThen(Expression.NotEqual(Expression.Property(p__src, pi), Expression.Constant(null)),
                                      Expression.Assign(
                                          Expression.Property(p__desc, pp),
                                          Expression.TypeAs(Expression.Call(
                                              Expression.MakeIndex(
                                                  Expression.Constant(GetObjectConvertTo(pp.PropertyType, 
                                                    pi.PropertyType)),
                                                  typeof(IDictionary<Type, Func<object, object>>).GetProperties()[0], new[] { Expression.Constant(clazz) }),
                                                  typeof(Func<object, object>).GetMethod("Invoke"), Expression.Property(p__src, pi)
                                              ), clazz)
                                          )
                                      )
                                  );
                        }
                        else
                        {
                            Type xe_desc = TypeTool.GetArrayElement(clazz);
                            Type xe_src = pi.PropertyType;
                            ParameterExpression size = Expression.Variable(typeof(int), "size");
                            ParameterExpression j = Expression.Variable(typeof(int), "j");
                            ParameterExpression d = Expression.Variable(typeof(Func<object, object>), "d");
                            ParameterExpression m = Expression.Variable(clazz, "m");
                            ParameterExpression item = Expression.Variable(xe_desc, "item");

                            Expression get = Expression.Assign(item, Expression.TypeAs(Expression.Call(d, typeof(Func<object, object>).GetMethod("Invoke"), Expression.MakeIndex(Expression.Property(p__src, pi), xe_src.GetProperty("Item", new[] { typeof(int) }), new[] { Expression.PostIncrementAssign(j) })), xe_desc));
                            Expression set = null;
                            if (clazz.IsArray)
                            {
                                set = Expression.MakeIndex(m, clazz.GetProperty("Item"), new[] { j });
                                set = Expression.Assign(item, set);
                            }
                            else
                            {
                                set = Expression.Call(m, clazz.GetMethod("Add"), new[] { item });
                            }
                            LabelTarget label = Expression.Label(typeof(int));
                            Expression exp = Expression.IfThen(Expression.NotEqual(Expression.Constant(null), Expression.Property(p__src, pi)),
                                Expression.Block(new[] { size, j, d, m, item },
                                    Expression.Assign(size, Expression.Property(Expression.Property(p__src, pi), "Length")),
                                    Expression.Assign(j, Expression.Constant(0)),
                                    Expression.Assign(d,
                                            Expression.MakeIndex(
                                                Expression.Constant(GetObjectConvertTo(pp.PropertyType, 
                                                    pi.PropertyType)),
                                                typeof(IDictionary<Type, Func<object, object>>).GetProperties()[0], new[] { Expression.Constant(xe_desc) })
                                            ),
                                    Expression.Assign(Expression.Property(p__desc, pp), Expression.New(clazz.GetConstructor(new Type[] { typeof(int) }), size)),
                                    Expression.Assign(m, Expression.Property(p__desc, pp)),
                                    Expression.Loop(Expression.IfThenElse(Expression.GreaterThanOrEqual(j, size),
                                                        Expression.Break(label, j),
                                                        Expression.Block(get, set))
                                    , label)
                                )
                            );
                            ls.Add(exp);
                        }
                    }
                }
            }
            ls.Add(Expression.Call(n, typeof(IList).GetMethod("Add"), p__desc));
            Expression safe_cast = Expression.MakeIndex(s, typeof(IList).GetProperties()[0], new[] { Expression.PostIncrementAssign(i) });
            if (src.IsClass)
                safe_cast = Expression.TypeAs(safe_cast, src);
            else
                safe_cast = Expression.Convert(safe_cast, src);
            Expression equals = (src.IsClass ? Expression.Equal(p__src, Expression.Constant(null)) : (Expression)Expression.Constant(false));
            ParameterExpression ss = Expression.Variable(typeof(IList), "ss");
            Expression<Func<IList, IList, IList>> ee = Expression.Lambda<Func<IList, IList, IList>>(Expression.Block(
                new[] { i, len },
                Expression.Assign(len, Expression.Property(s, typeof(ICollection).GetProperty("Count"))),
                Expression.Assign(i, Expression.Constant(0)),
                Expression.Loop(Expression.
                    IfThenElse(Expression.GreaterThanOrEqual(i, len),
                        Expression.Break(@break, i),
                        Expression.Block(
                            new[] { p__src, p__desc },
                            Expression.Assign(p__src, safe_cast),
                            Expression.IfThenElse(equals,
                                Expression.Block(
                                    Expression.Call(n, typeof(IList).GetMethod("Add"), desc.IsClass ? (Expression)Expression.Constant(null) : Expression.Default(desc)),
                                    Expression.MakeGoto(GotoExpressionKind.Continue, @continue, null, null)
                                ),
                                Expression.Block(
                                    ls.ToArray()
                                )
                            )
                        )
                    ), @break, @continue
                ), n
            ), new[] { s, n });
            return ee.Compile();
        }

        /// <summary>
        /// 快速拷贝列表
        /// </summary>
        /// <typeparam name="TSrc">源列表类型</typeparam>
        /// <typeparam name="TDesc">目标列表类型</typeparam>
        /// <param name="src">源集合</param>
        /// <param name="desc">目标集合</param>
        public static void CopyList<TSrc, TDesc>(IList<TSrc> src, IList<TDesc> desc)
        {
            if (src == null || desc == null)
                throw new ArgumentNullException();
            Type key = typeof(TDesc);
            Func<IList, IList, IList> f = GetListCopyProxy(key, typeof(TSrc));
            f((IList)src, (IList)desc);
        }

        private static Func<object, object> GetObjectCopyProxy(Type desc, Type src)
        {
            lock (m_pLook)
            {
                IDictionary<Type, Func<object, object>> convertto = null;
                if (!m_pRegTObjCopys.TryGetValue(src, out convertto))
                {
                    return null;
                }
                Func<object, object> f = null;
                if (convertto.TryGetValue(desc, out f))
                {
                    return f;
                }
                return null;
            }
        }

        private static Func<IList, IList, IList> GetListCopyProxy(Type desc, Type src)
        {
            lock (m_pLook)
            {
                IDictionary<Type, Func<IList, IList, IList>> convertto = null;
                if (m_pRegTListCopys.TryGetValue(src, out convertto))
                {
                    Func<IList, IList, IList> f = null;
                    if (convertto.TryGetValue(desc, out f))
                    {
                        return f;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 快速拷贝列表
        /// </summary>
        /// <typeparam name="TDesc">目标列表类型</typeparam>
        /// <param name="src">源集合</param>
        [Obsolete]
        public static IList<TDesc> CopyList<TDesc>(IList src)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }
            List<TDesc> results = new List<TDesc>(src.Count);
            if (src.Count > 0)
            {
                Type elemt = TypeTool.GetArrayElement(src.GetType());
                Type key = typeof(TDesc);
                Func <IList, IList, IList> f = GetListCopyProxy(key, elemt);
                f(src, results);
            }
            return results;
        }
        /// <summary>
        /// 快速拷贝列表
        /// </summary>
        /// <typeparam name="TDesc">目标列表类型</typeparam>
        /// <typeparam name="TSrc">源列表类型</typeparam>
        /// <param name="src">源集合</param>
        /// <returns></returns>
        public static IList<TDesc> CopyList<TDesc, TSrc>(IList<TSrc> src)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }
            List<TDesc> results = new List<TDesc>(src.Count);
            if (src.Count > 0)
            {
                Type key = typeof(TDesc);
                Func<IList, IList, IList> f = GetListCopyProxy(key, typeof(TSrc));
                f((IList)src, results);
            }
            return results;
        }
        /// <summary>
        /// 快速拷贝对象
        /// </summary>
        /// <typeparam name="TDesc">目标类型</typeparam>
        /// <param name="value">源对象</param>
        /// <returns></returns>
        public static TDesc Copy<TDesc>(this object value) where TDesc : class, new()
        {
            if (value == null)
                return null;
            Type key = typeof(TDesc);
            Func<object, object> d = GetObjectCopyProxy(key, value.GetType());
            return d(value) as TDesc;
        }
        /// <summary>
        /// 快速拷贝对象
        /// </summary>
        /// <typeparam name="TDesc">目标类型</typeparam>
        /// <typeparam name="TSrc">源类型</typeparam>
        /// <param name="value">源对象</param>
        /// <returns></returns>
        public static TDesc Copy<TDesc, TSrc>(this TSrc value) where TDesc : class, new()
        {
            if (value == null)
                return null;
            Type key = typeof(TDesc);
            Func<object, object> d = GetObjectCopyProxy(key, typeof(TSrc));
            return d(value) as TDesc;
        }

        public static void Load(Assembly[] assemblys)
        {
            if (assemblys == null)
            {
                throw new ArgumentNullException();
            }
            if (assemblys.Length <= 0)
            {
                throw new ArgumentException();
            }
            foreach (Assembly assembly in assemblys)
            {
                Load(assembly);
            }
        }

        public static void Load(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            foreach (Type type in assembly.GetExportedTypes())
            {
                IList<ModelConverterAttribute> attributes = Attributes.
                     GetAttributes<ModelConverterAttribute>(type);
                if (attributes.IsNullOrEmpty())
                {
                    continue;
                }
                foreach (ModelConverterAttribute attribute in attributes)
                {
                    if (attribute.Mode == ModelConverterAttribute.ConversionMode.Object)
                        RegisterCopy(attribute.Conversion, type);
                    else
                        RegisterCopyList(attribute.Conversion, type);
                }
            }
        }
    }
}
