namespace nsjsdotnet.Core.Data.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Collections.Concurrent;

    public class DatabaseConnectionPoll
    {
        private readonly IDictionary<int, IDbConnection> m_connections = null;
        private readonly Timer m_maintenance = null;
        private volatile Func<IDbConnection> m_newConnection = null;

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern int GetCurrentThreadId();

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr OpenThread(int dwDesiredAccess, int bInheritHandle, int dwThreadId);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern void CloseHandle(IntPtr handle);

            private const int THREAD_ALL_ACCESS = 2032639;

            public static int CurrentThreadId
            {
                get
                {
                    return GetCurrentThreadId();
                }
            }

            public static bool Exists(int threadId)
            {
                IntPtr hThread = OpenThread(THREAD_ALL_ACCESS, 0, threadId);
                if (hThread == IntPtr.Zero)
                    return false;
                CloseHandle(hThread);
                return true;
            }
        }

        [SecuritySafeCritical]
        public DatabaseConnectionPoll()
        {
            m_connections = new ConcurrentDictionary<int, IDbConnection>();
            m_maintenance = new Timer(Maintenance, null, 0, 500);
        }

        private void Maintenance(object state)
        {
            lock (this)
            {
                foreach (KeyValuePair<int, IDbConnection> pair in m_connections)
                {
                    try
                    {
                        bool reconstruction = false;
                        IDbConnection connection = pair.Value;
                        try
                        {
                            if (!NativeMethods.Exists(pair.Key))
                            {
                                if (connection != null)
                                    connection.Close();
                                m_connections.Remove(pair.Key);
                                continue;
                            }
                            if (connection == null)
                                reconstruction = true;
                            else
                                DatabaseAccessAuxiliary.ConnectConnection(pair.Value);
                        }
                        catch (Exception)
                        {
                            if (connection != null)
                                connection.Close();
                            reconstruction = true;
                        }
                        if (reconstruction && m_newConnection != null)
                        {
                            connection = m_newConnection();
                            m_connections[pair.Key] = connection;
                        }
                    }
                    catch (Exception) { /*maintenance*/ }
                }
            }
        }

        public IDbConnection Get(Func<IDbConnection> newConnection)
        {
            if (newConnection == null)
            {
                throw new ArgumentNullException();
            }
            else
            {
                m_newConnection = newConnection;
            }
            IDbConnection connection = null;
            lock (this)
            {
                int threadid = NativeMethods.CurrentThreadId;
                if (!m_connections.TryGetValue(threadid, out connection))
                {
                    connection = newConnection();
                    if (connection != null)
                    {
                        m_connections.Add(threadid, connection);
                    }
                }
            }
            return connection;
        }

        public int Count
        {
            get
            {
                return m_connections.Count;
            }
        }

        public IDbConnection Remove()
        {
            lock (this)
            {
                int threadid = Thread.CurrentThread.ManagedThreadId;
                IDbConnection connection = null;
                if (m_connections.TryGetValue(threadid, out connection))
                {
                    m_connections.Remove(threadid);
                }
                return connection;
            }
        }
    }
}
