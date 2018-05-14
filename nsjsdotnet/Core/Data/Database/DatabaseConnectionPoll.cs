namespace nsjsdotnet.Core.Data.Database
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Security;

    public class DatabaseConnectionPoll
    {
        private readonly IDictionary<IRelational, IDbConnection> m_connections = null;
        private volatile Func<IDbConnection> m_newConnection = null;
        private readonly EventHandler m_closeRelationalEvt = null;

        [SecuritySafeCritical]
        public DatabaseConnectionPoll()
        {
            m_connections = new ConcurrentDictionary<IRelational, IDbConnection>();
            m_closeRelationalEvt = this.OnCloseRelational;
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
                m_newConnection = newConnection;
            }
            lock (this)
            {
                IDbConnection connection = null;
                if (!m_connections.TryGetValue(relational, out connection))
                {
                    connection = newConnection();
                    if (connection != null)
                    {
                        relational.Disposed += this.m_closeRelationalEvt;
                        m_connections.Add(relational, connection);
                    }
                }
                return DatabaseAccessAuxiliary.ConnectConnection(connection);
            }
        }

        public int Count
        {
            get
            {
                return m_connections.Count;
            }
        }

        public IDbConnection Remove(IRelational relational)
        {
            if (relational == null)
            {
                throw new ArgumentNullException("relational");
            }
            lock (this)
            {
                IDbConnection connection = null;
                if (m_connections.TryGetValue(relational, out connection))
                {
                    relational.Disposed -= this.m_closeRelationalEvt;
                    m_connections.Remove(relational);
                    try
                    {
                        connection.Close();
                    }
                    catch (Exception) { /*-------------------------*/ }
                }
                return connection;
            }
        }
    }
}
