namespace nsjsdotnet.Core.Data.Database
{
    using nsjsdotnet.Core.Data.Converter;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    public class DataTableGateway
    {
        public DatabaseAccessAdapter DatabaseAccessAdapter
        {
            get;
            private set;
        }

        public DataTableGateway(DatabaseAccessAdapter adapter)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException("adapter");
            }
            DatabaseAccessAdapter = adapter;
        }

        private IList<T> ToList<T>(DataTable table) where T : class
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            DataRowCollection rows = table.Rows;
            if (rows.Count <= 0)
            {
                return new List<T>(0);
            }
            if (typeof(T) == typeof(object))
            {
                var proxy = DataModelProxyConverter.GetInstance();
                return (IList<T>)proxy.ToList(table);
            }
            else
            {
                return DataModelConverter.ToList<T>(table);
            }
        }

        public IDbTransaction CreateTransaction()
        {
            return DatabaseAccessAdapter.CreateTransaction();
        }

        public DataTable Select(IDbCommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            return DatabaseAccessAuxiliary.FillDataTable(cmd, DatabaseAccessAdapter);
        }

        public IList<T> Select<T>(IDbCommand cmd) where T : class
        {
            using (DataTable dt = this.Select(cmd))
            {
                if (dt != null)
                {
                    return this.ToList<T>(dt);
                }
                return default(IList<T>);
            }
        }

        public DataTable Select(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }
            if (sql.Length <= 0)
            {
                throw new ArgumentException("sql");
            }
            using (IDbCommand cmd = DatabaseAccessAdapter.CreateCommand(sql))
            {
                return this.Select(cmd);
            }
        }

        public DataTable Select(StringBuilder sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }
            return this.Select(sql.ToString());
        }

        public DataTable GetByKey(string table, string primarykey, string rowkey)
        {
            if (rowkey == null)
            {
                throw new ArgumentNullException("rowkey");
            }
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (primarykey == null)
            {
                throw new ArgumentNullException("primarykey");
            }
            if (rowkey.Length <= 0)
            {
                throw new ArgumentException("rowkey");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            if (primarykey.Length <= 0)
            {
                throw new ArgumentException("primarykey");
            }
            using (IDbCommand cmd = DatabaseAccessAdapter.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT TOP 1 * FROM [{0}] WHERE {1}={2}", table, primarykey, rowkey);
                return this.Select(cmd);
            }
        }

        public T GetByKey<T>(string table, string primarykey, string rowkey) where T : class
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }
            if (string.IsNullOrEmpty(primarykey))
            {
                throw new ArgumentNullException("primarykey");
            }
            if (string.IsNullOrEmpty(rowkey))
            {
                throw new ArgumentNullException("rowkey");
            }
            using (DataTable dt = GetByKey(table, primarykey, rowkey))
            {
                if (dt == null)
                {
                    return null;
                }
                IList<T> rows = ToList<T>(dt);
                if (rows == null || rows.Count <= 0)
                {
                    return null;
                }
                return rows[0];
            }
        }

        public IList<T> FindAll<T>(string table, Expression<Func<T, bool>> expression) where T : class
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            if (expression == null)
            {
                return this.Select<T>(string.Format("SELECT * FROM [{0}]", table));
            }
            else
            {
                string condition = ExpressionSpecification.SatisfiedBy(expression, table);
                return this.Select<T>(string.Format("SELECT * FROM [{0}] WHERE {1}", table, condition));
            }
        }

        public T Find<T>(string table, Expression<Func<T, bool>> expression) where T : class
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            IList<T> rows = default(IList<T>);
            if (expression == null)
            {
                rows = Select<T>(string.Format("SELECT TOP 1 * FROM [{0}]", table));
            }
            else
            {
                string condition = ExpressionSpecification.SatisfiedBy(expression, table);
                rows = Select<T>(string.Format("SELECT TOP 1 * FROM [{0}] WHERE {1}", table, condition));
            }
            if (rows == null)
            {
                return default(T);
            }
            return rows.FirstOrDefault();
        }

        public IList<T> Select<T>(string sql) where T : class
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }
            if (sql.Length <= 0)
            {
                throw new ArgumentException("sql");
            }
            using (IDbCommand cmd = DatabaseAccessAdapter.CreateCommand(sql))
            {
                return this.Select<T>(cmd);
            }
        }

        public IList<T> Select<T>(StringBuilder sql) where T : class, new()
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }
            return this.Select<T>(sql.ToString());
        }

        public DataTable Select(string procedure, params IDbDataParameter[] parameters)
        {
            if (procedure == null)
            {
                throw new ArgumentNullException("procedure");
            }
            if (procedure.Length <= 0)
            {
                throw new ArgumentException("procedure");
            }
            using (IDbCommand cmd = DatabaseAccessAdapter.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procedure;
                //
                if (parameters != null)
                {
                    foreach (IDbDataParameter parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }
                }
                return this.Select(cmd);
            }
        }

        public IList<T> Select<T>(string procedure, params IDbDataParameter[] parameters) where T : class, new()
        {
            using (DataTable dt = this.Select(procedure, parameters))
            {
                return this.ToList<T>(dt);
            }
        }

        public DataTable Select(string procedure, object parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }
            return this.Select(procedure, DatabaseAccessAuxiliary.GetParameters(parameter, DatabaseAccessAdapter));
        }

        public IList<T> Select<T>(string procedure, object parameter) where T : class, new()
        {
            using (DataTable dt = this.Select(procedure, parameter))
            {
                return this.ToList<T>(dt);
            }
        }

        public int Count(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("sql");
            }
            using (DataTable dt = this.Select(sql))
            {
                DataRowCollection rows = dt.Rows;
                if (rows.Count > 0)
                {
                    return Convert.ToInt32(rows[0][0]);
                }
                return default(int);
            }
        }

        public int Count(StringBuilder sql)
        {
            if (sql == null)
            {
                throw new ArgumentException("sql");
            }
            return this.Count(sql.ToString());
        }

        public int Count<T>(string table, Expression<Func<T, bool>> expression) where T : class
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }
            if (expression == null)
            {
                return this.Count(table, string.Empty);
            }
            else
            {
                string condition = ExpressionSpecification.SatisfiedBy(expression, table);
                return this.Count(table, condition);
            }
        }

        public bool Contains(string table, string primarykey, string rowkey)
        {
            if (primarykey == null)
            {
                throw new ArgumentNullException("primarykey");
            }
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (rowkey == null)
            {
                throw new ArgumentNullException("rowkey");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            if (primarykey.Length <= 0)
            {
                throw new ArgumentException("primarykey");
            }
            if (rowkey.Length <= 0)
            {
                throw new ArgumentException("rowkey");
            }
            return this.InternalGetCount(table, 1, string.Format("{0}={1}", primarykey, rowkey)) > 0;
        }

        public bool Contains<T>(string table, Expression<Func<T, bool>> expression)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            if (expression == null)
            {
                return this.InternalGetCount(table, 1, string.Empty) > 0;
            }
            else
            {
                string condition = ExpressionSpecification.SatisfiedBy(expression, table);
                if (string.IsNullOrEmpty(condition))
                {
                    return false;
                }
                return this.InternalGetCount(table, 1, condition) > 0;
            }
        }

        public int Count(string table, string condition)
        {
            return this.InternalGetCount(table, null, condition);
        }

        private int InternalGetCount(string table, int? top, string condition)
        {
            string sql = string.Format("SELECT COUNT(1) FROM [{0}] ", table);
            if (!string.IsNullOrEmpty(condition))
            {
                sql += string.Format("WHERE {0}", condition);
            }
            return this.Count(sql);
        }

        public int Insert(string table, object rowdata)
        {
            return this.Insert(table, rowdata, null);
        }

        public int Insert(string table, object rowdata, IDbTransaction transaction)
        {
            if (rowdata == null)
            {
                throw new ArgumentNullException("rowdata");
            }
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            using (IDbCommand cmd = DatabaseAccessAuxiliary.CreateInsert(rowdata, table, DatabaseAccessAdapter))
            {
                cmd.Transaction = transaction;
                return ExecuteNonQuery(cmd);
            }
        }

        public int Update(string table, string primarykey, string condition, object rowdata)
        {
            return this.Update(table, primarykey, condition, rowdata, null);
        }

        public int Update(string table, string primarykey, string condition, object rowdata, IDbTransaction transaction)
        {
            if (primarykey == null)
            {
                throw new ArgumentNullException("primarykey");
            }
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (rowdata == null)
            {
                throw new ArgumentNullException("rowdata");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            if (primarykey.Length <= 0)
            {
                throw new ArgumentException("primarykey");
            }
            using (IDbCommand cmd = DatabaseAccessAuxiliary.CreateUpdate(rowdata, table, primarykey, condition, DatabaseAccessAdapter))
            {
                cmd.Transaction = transaction;
                return ExecuteNonQuery(cmd);
            }
        }

        public int DeleteAll(string table)
        {
            return this.DeleteAll(table, null);
        }

        public int DeleteAll(string table, IDbTransaction transaction)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (table.Length <= 0)
            {
                throw new ArgumentException("table");
            }
            using (IDbCommand cmd = DatabaseAccessAdapter.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = string.Format("DELETE FROM [{0}]", table);
                return ExecuteNonQuery(cmd);
            }
        }

        public int DeleteByKey(string table, string primarykey, string rowkey)
        {
            return this.DeleteByKey(table, primarykey, rowkey);
        }

        public int DeleteByKey(string table, string primarykey, string rowkey, IDbTransaction transaction)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(primarykey) || string.IsNullOrEmpty(rowkey))
            {
                throw new ArgumentException();
            }
            using (IDbCommand cmd = DatabaseAccessAdapter.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = string.Format("DELETE FROM [{0}] WHERE {1}={2}", table, primarykey, rowkey);
                return ExecuteNonQuery(cmd);
            }
        }

        public int ExecuteNonQuery(IDbCommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            return DatabaseAccessAuxiliary.ExecuteNonQuery(cmd, DatabaseAccessAdapter);
        }

        public object ExecuteScalar(IDbCommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            return DatabaseAccessAuxiliary.ExecuteScalar(cmd, DatabaseAccessAdapter);
        }

        public IDataReader ExecuteReader(IDbCommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            return DatabaseAccessAuxiliary.ExecuteReader(cmd, DatabaseAccessAdapter);
        }
    }
}
