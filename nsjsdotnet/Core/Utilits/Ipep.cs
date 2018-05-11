namespace nsjsdotnet.Core.Utilits
{
    using nsjsdotnet.Core.Diagnostics;
    using System;
    using System.Net;

    public static class Ipep
    {
        public static IPEndPoint ToIpep(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length <= 0)
            {
                throw new ArgumentException("address");
            }
            int index = address.IndexOf(':');
            Contract.Requires<ArgumentException>(index > -1);
            string host = address.Substring(0, index++);
            int port = 0;
            Contract.Requires<ArgumentException>(int.TryParse(address.Substring(index), out port));
            return new IPEndPoint(IPAddress.Parse(host), port);
        }

        public static string ToIpepString(string host, int port)
        {
            return string.Format("{0}:{1}", host, port);
        }

        public static string GetHostName(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length <= 0)
            {
                throw new ArgumentException("address");
            }
            int index = address.IndexOf(':');
            Contract.Requires<ArgumentException>(index > -1);
            return address.Substring(0, index++);
        }

        public static int GetPort(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length <= 0)
            {
                throw new ArgumentException("address");
            }
            int index = address.IndexOf(':');
            Contract.Requires<ArgumentException>(index > -1);
            int port = 0;
            Contract.Requires<ArgumentException>(int.TryParse(address.Substring(++index), out port));
            return port;
        }
    }
}
