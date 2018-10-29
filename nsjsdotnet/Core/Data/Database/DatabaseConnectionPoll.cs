namespace nsjsdotnet.Core.Data.Database
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Security;

    public class DatabaseConnectionPoll
    {
        private readonly IDictionary<IRelational, IDbConnection> _connections = null;
        private volatile Func<IDbConnection> _newConnection = null;
        private readonly EventHandler _closeRelationalEvt = null;

        [SecuritySafeCritical]
        public DatabaseConnectionPoll()
        {
            this._connections = new ConcurrentDictionary<IRelational, IDbConnection>();
            this._closeRelationalEvt = this.OnCloseRelational;
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
            lock (this._connections)
            {
                IDbConnection connection = null;
                if (!this._connections.TryGetValue(relational, out connection))
                {
                    connection = newConnection();
                    if (!DatabaseAccessAuxiliary.TryConnectConnection(connection))
                    {
                        DatabaseAccessAuxiliary.CloseConnection(connection);
                    }
                    else
                    {
                        relational.Disposed += this._closeRelationalEvt;
                        this._connections.Add(relational, connection);
                    }
                }
                return connection;
            }
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
            lock (this._connections)
            {
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
}
