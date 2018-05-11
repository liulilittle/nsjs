namespace nsjsdotnet.Core.Data.Converter
{
    using nsjsdotnet.Core.Utilits;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed partial class DataModelProxyConverter
    {
        private AssemblyBuilder _assemblyBuilder = null;
        private ModuleBuilder _moduleBuilder = null;
        private ConcurrentDictionary<DataColumnCollection, Type> _dynamicTypeDictionary = null;

        private DataModelProxyConverter()
        {
            AssemblyName assemblyName = new AssemblyName("Platform.Aop.Dynamic");
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            _dynamicTypeDictionary = new ConcurrentDictionary<DataColumnCollection, Type>();
        }

        private Type GetDynamicType(DataColumnCollection columns)
        {
            if (columns == null || columns.Count <= 0)
            {
                return null;
            }
            foreach (KeyValuePair<DataColumnCollection, Type> pair in _dynamicTypeDictionary)
            {
                DataColumnCollection key = pair.Key;
                if (key.Count == columns.Count)
                {
                    int equals = 0;
                    for (int i = 0; i < key.Count; i++)
                    {
                        string x = key[i].ColumnName.ToLower(), y = columns[i].ColumnName.ToLower();
                        if (key[i].DataType == columns[i].DataType && x == y)
                        {
                            equals++;
                        }
                    }
                    if (equals == key.Count)
                    {
                        return pair.Value;
                    }
                }
            }
            return null;
        }

        private Type CreateDynamicType(DataColumnCollection columns)
        {
            if (columns == null || columns.Count <= 0)
            {
                return null;
            }
            string strNewSha1TypeName = Path.GetRandomFileName();
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(strNewSha1TypeName, TypeAttributes.Public);
            foreach (DataColumn column in columns)
            {
                Type colDataType = column.DataType;
                if (column.AllowDBNull && colDataType.IsValueType)
                {
                    colDataType = typeof(Nullable<>).MakeGenericType(colDataType);
                }
                FieldBuilder ldFieldBuilder = typeBuilder.DefineField(column.ColumnName, colDataType, FieldAttributes.Private);
                MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(string.Format("get_{0}", colDataType),
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, colDataType, Type.EmptyTypes);
                ILGenerator il = getMethodBuilder.GetILGenerator();
                //
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, ldFieldBuilder);
                il.Emit(OpCodes.Ret);
                //
                MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(string.Format("set_{0}", column.ColumnName),
                      MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeof(void), new Type[] { colDataType });
                il = setMethodBuilder.GetILGenerator();
                //
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, ldFieldBuilder);
                il.Emit(OpCodes.Ret);
                //
                PropertyBuilder sPropertyBuilder = typeBuilder.DefineProperty(column.ColumnName, System.Reflection.PropertyAttributes.SpecialName, colDataType, Type.EmptyTypes);
                sPropertyBuilder.SetSetMethod(setMethodBuilder);
                sPropertyBuilder.SetGetMethod(getMethodBuilder);
            }
            return typeBuilder.CreateType();
        }

        public Type GetType(DataTable table)
        {
            if (table == null)
            {
                return null;
            }
            DataColumnCollection columns = table.Columns;
            if (columns.Count <= 0)
            {
                return null;
            }
            Type dynamicType = GetDynamicType(columns);
            if (dynamicType == null)
            {
                dynamicType = CreateDynamicType(columns);
                if (dynamicType != null)
                {
                    _dynamicTypeDictionary.TryAdd(columns, dynamicType);
                }
            }
            return dynamicType;
        }

        public object GetObject(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public IList<object> ToList(DataTable value, Type type = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            if (type == null)
            {
                type = GetType(value);
                if (type == null)
                {
                    throw new ArgumentNullException();
                }
            }
            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new ArgumentNullException();
            }
            IList<KeyValuePair<PropertyInfo, int>> properties = new List<KeyValuePair<PropertyInfo, int>>();
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                foreach (DataColumn cols in value.Columns)
                {
                    if ((cols.ColumnName).ToUpper() == (property.Name).ToUpper())
                    {
                        properties.Add(new KeyValuePair<PropertyInfo, int>(property, cols.Ordinal));
                    }
                }
            }
            IList<object> buffer = new List<object>();
            foreach (DataRow row in value.Rows)
            {
                object model = ctor.Invoke(Type.EmptyTypes);
                try
                {
                    buffer.Add(model);
                }
                finally
                {
                    foreach (KeyValuePair<PropertyInfo, int> pair in properties)
                    {
                        object args = row[pair.Value];
                        if (args == DBNull.Value)
                        {
                            args = null;
                        }
                        Type prop = pair.Key.PropertyType;
                        if (args != null && !prop.IsInstanceOfType(args))
                        {
                            if (prop.IsEnum)
                            {
                                prop = prop.GetEnumUnderlyingType();
                            }
                            if (TypeTool.IsFloat(prop))
                            {
                                args = ValuetypeFormatter.Parse(Convert.ToString(args), prop, NumberStyles.Number | NumberStyles.Float);
                            }
                            else
                            {
                                args = ValuetypeFormatter.Parse(Convert.ToString(args), prop);
                            }
                        }
                        pair.Key.SetValue(model, args, null);
                    }
                }
            }
            return buffer;
        }

        private static DataModelProxyConverter _aopDynamicProxy = new DataModelProxyConverter();

        public static DataModelProxyConverter GetInstance()
        {
            return _aopDynamicProxy;
        }
    }
}
