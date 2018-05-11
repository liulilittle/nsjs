namespace nsjsdotnet.Core.Data.Database
{
    using System;
    using System.Data;

    public abstract class DatabaseAccessAdapter
    {
        protected string Server
        {
            get;
            private set;
        }

        protected string Database
        {
            get;
            private set;
        }

        protected string LoginId
        {
            get;
            private set;
        }

        protected string Password
        {
            get;
            private set;
        }

        public object Tag
        {
            get;
            set;
        }

        public abstract IDbConnection GetConnection();

        public abstract IDbCommand CreateCommand();

        public abstract IDbCommand CreateCommand(string sql);

        public abstract IDbDataAdapter CreateAdapter();

        public abstract IDbDataParameter CreateParameter();

        public abstract IDbDataParameter CreateParameter(string name, object value);

        public abstract IDbDataParameter[] GetParameters(string procedure);

        protected DatabaseConnectionPoll Poll
        {
            get;
            private set;
        }

        protected DatabaseAccessAdapter(string server, string database, string loginId, string password)
        {
            if (string.IsNullOrEmpty(server) ||
                string.IsNullOrEmpty(database) ||
                string.IsNullOrEmpty(loginId) ||
                string.IsNullOrEmpty(password))
            {
                throw new ArgumentException();
            }
            Server = server;
            Database = database;
            LoginId = loginId;
            Password = password;
            Poll = new DatabaseConnectionPoll();
        }

        public IDbTransaction CreateTransaction()
        {
            IDbConnection connection = GetConnection();
            DatabaseAccessAuxiliary.ConnectConnection(connection);
            return connection.BeginTransaction();
        }
    }
}
