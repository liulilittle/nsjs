namespace nsjsdotnet.Core.Data.Database
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Security;
    using nsjsdotnet.Core.Threading;

    public class DatabaseConnectionPoll
    {
        private readonly IDictionary<IRelational, IDbConnection> _connections = null;
        private volatile Func<IDbConnection> _newConnection = null;
        private readonly EventHandler _closeRelationalEvt = null;
        private readonly Timer _maintenance = null;

        [SecuritySafeCritical]
        public DatabaseConnectionPoll()
        {
            this._connections = new ConcurrentDictionary<IRelational, IDbConnection>();
            this._closeRelationalEvt = this.OnCloseRelational;
            this._maintenance = new Timer(1000);
            this._maintenance.Tick += this.Maintenance;
            this._maintenance.Start();
        }

        private void Maintenance(object sender, EventArgs e)
        {
            foreach (KeyValuePair<IRelational, IDbConnection> pair in this._connections)
            {
                IDbConnection connection = pair.Value;
                IRelational relational = pair.Key;
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    this.Remove(relational);
                }
            }
        }

        private void OnCloseRelational(object sender, EventArgs e)
        {
            IRelational relational = sender as IRelational;
            if (relational != null)
            {
                this.Remove(relational);
            }
        }

        public IDbConnection Get(IRelational relational, Func<IDbConnection> newConnection)
        {
            if (relational == null)
            {
                throw new ArgumentNullException("relational");
            }
            if (newConnection == null)
            {
                throw new ArgumentNullException("newConnection");
            }
            else
            {
                this._newConnection = newConnection;
            }
            IDbConnection connection = null;
            if (!this._connections.TryGetValue(relational, out connection))
            {
                connection = newConnection();
                if (!DatabaseAccessAuxiliary.TryConnectConnection(connection))
                {
                    DatabaseAccessAuxiliary.CloseConnection(connection);
                    connection = null;
                }
                else
                {
                    relational.Disposed += this._closeRelationalEvt;
                    this._connections.Add(relational, connection);
                }
            }
            return connection;
        }

        public int Count
        {
            get
            {
                return this._connections.Count;
            }
        }

        public virtual IDbConnection Remove(IRelational relational)
        {
            if (relational == null)
            {
                throw new ArgumentNullException("relational");
            }
            IDbConnection connection = null;
            if (this._connections.TryGetValue(relational, out connection))
            {
                relational.Disposed -= this._closeRelationalEvt;
                this._connections.Remove(relational);
            }
            DatabaseAccessAuxiliary.CloseConnection(connection);
            return connection;
        }
    }
}
