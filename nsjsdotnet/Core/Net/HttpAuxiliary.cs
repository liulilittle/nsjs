namespace nsjsdotnet.Core.Net
{
    using System;
    using System.Net;
    using System.Text;

    public static class HttpAuxiliary
    {
        public static bool TryReadAllText(string path, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Encoding = Encoding.UTF8;
                    value = client.DownloadString(path);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
