namespace nsjsdotnet.Core.Net.Web
{
    using System.IO;

    public class HttpPostedFile 
    {
        public long ContentLength
        {
            get;
            internal set;
        }

        public string ContentType
        {
            get;
            internal set;
        }

        public string FileName
        {
            get;
            internal set;
        }

        public Stream InputStream
        {
            get;
            internal set;
        }

        public void SaveAs(string path)
        {
            Stream s = this.InputStream;
            if (s != null)
            {
                using (FileStream f = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    s.CopyTo(f);
                    f.Flush();
                }
            }
        }
    }
}
