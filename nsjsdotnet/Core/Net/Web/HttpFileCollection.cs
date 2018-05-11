namespace nsjsdotnet.Core.Net.Web
{
    using System.Collections.Specialized;

    public class HttpFileCollection : NameObjectCollectionBase
    {
        internal HttpFileCollection()
        {
           
        }

        internal void Add(string name, HttpPostedFile file)
        {
            base.BaseAdd(name, file);
        }

        public string[] AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public HttpPostedFile this[int index]
        {
            get
            {
                return base.BaseGet(index) as HttpPostedFile;
            }
        }

        public HttpPostedFile this[string name]
        {
            get
            {
                return base.BaseGet(name) as HttpPostedFile;
            }
        }
    }
}
