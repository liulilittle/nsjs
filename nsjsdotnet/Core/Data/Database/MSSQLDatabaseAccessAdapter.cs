namespace nsjsdotnet.Core.Data.Database
{
    using System.Data;
    using System.Data.SqlClient;

    public class MSSQLDatabaseAccessAdapter : DatabaseAccessAdapter
    {
        public MSSQLDatabaseAccessAdapter(string server, string database, string loginId, string password) : 
            base(server, database, loginId, password)
        {
        }

        public override string ToString()
        {
            return string.Format("Data Source={0}; Initial Catalog={1}; User Id={2}; Password={3};", this.Server, this.Database, this.LoginId, this.Password);
        }

        public override IDbConnection GetConnection()
        {
            return Poll.Get(() =>
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = this.ToString();
                return connection;
            });
        }

        public override IDbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public override IDbDataAdapter CreateAdapter()
        {
            return new SqlDataAdapter();
        }

        public override IDbDataParameter CreateParameter()
        {
            return new SqlParameter();
        }

        public override IDbDataParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }

        public override IDbDataParameter[] GetParameters(string procedure)
        {
            return DatabaseAccessAuxiliary.GetParameters<SqlConnection, SqlCommand>(procedure, this, (cmd) => SqlCommandBuilder.
                DeriveParameters(cmd));
        }

        public override IDbCommand CreateCommand(string sql)
        {
            return new SqlCommand(sql);
        }
    }
}
