namespace nsjsdotnet.Core.Net
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Mail;
    using TIMEOUT = System.Threading.Timeout;

    public class MailClient
    {
        private SendCompletedEventHandler g_SendCompletedProc;
        public const int DefaultTimeout = TIMEOUT.Infinite;
        public const int DefaultPort = 25;

        public virtual bool EnableSsl
        {
            get;
            set;
        }

        public virtual int Timeout
        {
            get;
            set;
        }

        public virtual string Server
        {
            get;
            private set;
        }

        public virtual int Port
        {
            get;
            private set;
        }

        public NetworkCredential Credentials
        {
            get;
            private set;
        }

        public MailClient(string server, int port, string username, string password) :
            this(server, port, username, password, null)
        {

        }

        public MailClient(string server, int port, string username, string password, string domain)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            if (username.Length <= 0)
            {
                throw new ArgumentException("username");
            }
            if (password.Length <= 0)
            {
                throw new ArgumentException("password");
            }
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (server.Length <= 0)
            {
                throw new ArgumentException("server");
            }
            this.Port = port;
            this.Server = server;
            this.Timeout = DefaultTimeout;
            this.Credentials = new NetworkCredential(username, password, domain);
        }

        public virtual bool Send(MailMessage message)
        {
            if (message == null)
            {
                return false;
            }
            using (SmtpClient smtp = this.NewSmtpClient(false))
            {
                smtp.Send(message);
                return true;
            }
        }

        protected virtual SmtpClient NewSmtpClient(bool async)
        {
            SmtpClient smtp = new SmtpClient(this.Server, this.Port);
            smtp.UseDefaultCredentials = true;
            smtp.EnableSsl = this.EnableSsl;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = this.Credentials;
            if (this.Timeout != DefaultTimeout)
            {
                smtp.Timeout = this.Timeout;
            }
            if (async)
            {
                lock (this)
                {
                    if (this.g_SendCompletedProc == null)
                    {
                        this.g_SendCompletedProc = this.SendCompleted;
                    }
                }
                smtp.SendCompleted += this.g_SendCompletedProc;
            }
            return smtp;
        }

        public virtual bool SendAsync(MailMessage message, Action<Exception> callback)
        {
            if (message == null)
            {
                return false;
            }
            SmtpClient smtp = this.NewSmtpClient(true);
            smtp.SendAsync(message, callback);
            return false;
        }

        private void SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SmtpClient smtp = sender as SmtpClient;
            if (smtp != null)
            {
                smtp.Dispose();
                Action<Exception> success = e.UserState as Action<Exception>;
                success?.Invoke(e.Error);
            }
        }
    }
}
