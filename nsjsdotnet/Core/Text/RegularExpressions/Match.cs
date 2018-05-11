namespace nsjsdotnet.Core.Text.RegularExpressions
{
    using System;
    using System.Reflection;

    public class Match
    {
        private Type ComType;
        private object ComObject;

        internal Match(object obj)
        {
            this.ComObject = obj; this.ComType = obj.GetType();
        }
        object _get(string name)
        {
            return this.ComType.InvokeMember(name, BindingFlags.GetProperty, null, this.ComObject, null);
        }
        object _set(string name, object args)
        {
            return this.ComType.InvokeMember(name, BindingFlags.SetProperty, null, this.ComObject, new object[] { args });
        }
        public int FirstIndex
        {
            get
            {
                var obj = this._get("FirstIndex");
                return (int)(obj is string ? 0 : obj);
            }
        }
        public int Length
        {
            get { return (int)this._get("Length"); }
        }
        public string Value
        {
            get { return this.ComObject is string ? this.ComObject.ToString() : this._get("Value").ToString(); }
        }
        public MatchCollection Groups
        {
            get { return new MatchCollection(this._get("SubMatches")); }
        }
    }
}
