namespace nsjsdotnet.Core.Data.Database
{
    using nsjsdotnet.Core.Linq;
    using System;
    using System.Data;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public static class DatabaseAccessAuxiliary
    {
        public static IDbConnection ConnectConnection(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException();
            }
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection;
        }

        public static int ExecuteNonQuery(IDbCommand cmd, DatabaseAccessAdapter adapter)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            if (adapter == null)
            {
                throw new ArgumentNullException("connection");
            }
            cmd.Connection = ConnectConnection(adapter.GetConnection());
            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(IDbCommand cmd, DatabaseAccessAdapter adapter)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            if (adapter == null)
            {
                throw new ArgumentNullException("connection");
            }
            cmd.Connection = ConnectConnection(adapter.GetConnection());
            return cmd.ExecuteScalar();
        }

        public static IDataReader ExecuteReader(IDbCommand cmd, DatabaseAccessAdapter adapter)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            if (adapter == null)
            {
                throw new ArgumentNullException("connection");
            }
            cmd.Connection = ConnectConnection(adapter.GetConnection());
            return cmd.ExecuteReader();
        }

        public static DataSet FillDataSet(IDbCommand cmd, DatabaseAccessAdapter adapter)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            if (adapter == null)
            {
                throw new ArgumentNullException("connection");
            }
            cmd.Connection = ConnectConnection(adapter.GetConnection());
            {
                DataSet ds = null;
                IDbDataAdapter da = adapter.CreateAdapter();
                {
                    da.SelectCommand = cmd;
                    ds = new DataSet();
                    da.Fill(ds);
                }
                IDisposable disposable = da as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                return ds;
            }
        }

        public static DataTable FillDataTable(IDbCommand cmd, DatabaseAccessAdapter adapter)
        {
            DataSet ds = FillDataSet(cmd, adapter);
            if (ds == null)
            {
                return null;
            }
            DataTableCollection dts = ds.Tables;
            if (dts.Count <= 0)
            {
                return null;
            }
            return dts[0];
        }

        public static IDbCommand CreateInsert(object value, string table, DatabaseAccessAdapter adapter)
        {
            if (value == null || string.IsNullOrEmpty(table))
            {
                throw new ArgumentException();
            }
            string sql = "INSERT INTO [{0}]({1}) VALUES({2})";
            IDbCommand cmd = adapter.CreateCommand();
            IDataParameterCollection args = cmd.Parameters;
            PropertyInfo[] props = (value.GetType()).GetProperties();
            int len = props.Length - 1;
            string fileds = null, values = null;
            for (int i = 0; i <= len; i++)
            {
                PropertyInfo prop = props[i];
                if (i >= len)
                {
                    values += ("@" + prop.Name);
                    fileds += string.Format("[{0}]", prop.Name);
                }
                else
                {
                    values += string.Format("@{0},", prop.Name);
                    fileds += string.Format("[{0}],", prop.Name);
                }
                object val = prop.GetValue(value, null);
                if (val == null)
                {
                    val = DBNull.Value;
                }
                args.Add(adapter.CreateParameter(string.Format("@{0}", prop.Name), val));
            }
            sql = string.Format(sql, table, fileds, values);
            cmd.CommandText = sql;
            return cmd;
        }

        public static IDbCommand CreateUpdate(object value, string table, string key, string where, DatabaseAccessAdapter adapter)
        {
            if (value == null || string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key))
            {
                throw new ArgumentException();
            }
            string when = string.Empty, sql = string.Format("UPDATE [{0}] SET", table);
            var cmd = adapter.CreateCommand();
            var args = cmd.Parameters;
            PropertyInfo[] props = (value.GetType()).GetProperties();
            int len = props.Length - 1;
            for (int i = 0; i <= len; i++)
            {
                PropertyInfo prop = props[i];
                if (!Regex.IsMatch(prop.Name, key, RegexOptions.IgnoreCase))
                {
                    sql += string.Format(i >= len ? "[{0}]=@{1}" : " [{0}]=@{1},", prop.Name, prop.Name);
                }
                else
                {
                    when += string.Format(" WHERE {0}=@{1}", key, key);
                }
                object val = prop.GetValue(value, null);
                if (val == null)
                {
                    val = DBNull.Value;
                }
                args.Add(adapter.CreateParameter(string.Format("@{0}", prop.Name), val));
            }
            if (!string.IsNullOrEmpty(where))
            {
                when += string.Format(" AND {0} ", where);
            }
            cmd.CommandText = (sql += when);
            return cmd;
        }

        public static bool InvalidParameter(params string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                return true;
            }
            foreach (string item in args)
            {
                if (item.IndexOf("'") > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static IDbDataParameter[] GetParameters(object value, DatabaseAccessAdapter adapter)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type clazz = value.GetType();
            PropertyInfo[] properties = clazz.GetProperties();
            IDbDataParameter[] parameters = new IDbDataParameter[properties.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                PropertyInfo prop = properties[i];
                object obj = prop.GetValue(value, null);
                if (obj == null)
                {
                    obj = DBNull.Value;
                }
                parameters[i] = adapter.CreateParameter(string.Format("@{0}", prop.Name), obj);
            }
            return parameters;
        }

        public static IDbDataParameter[] GetParameters<TConnection, TCommand>(string procedure, DatabaseAccessAdapter adapter, Action<TCommand> deriveParameters)
            where TConnection : IDbConnection
            where TCommand : IDbCommand, new()
        {
            using (TConnection connection = (TConnection)adapter.GetConnection())
            {
                using (TCommand cmd = new TCommand())
                {
                    cmd.CommandText = procedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    deriveParameters(cmd); // 探测存储过程参数                               
                    var args = cmd.Parameters;
                    var buffer = new IDbDataParameter[args.Count];
                    for (int i = 0; i < args.Count; i++) // 交接且设置默认值
                    {
                        IDbDataParameter arg = (IDbDataParameter)args[i];
                        arg.Value = DBNull.Value;
                        buffer[i] = arg;
                    }
                    return buffer;
                }
            }
        }
    }
}
