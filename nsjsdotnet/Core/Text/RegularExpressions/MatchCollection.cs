namespace nsjsdotnet.Core.Text.RegularExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class MatchCollection
    {
        private Type ComType;
        private object ComObject;

        internal MatchCollection(object obj)
        {
            this.ComObject = obj; this.ComType = obj.GetType();
        }
        object _invoke(string name, object[] args = null)
        {
            return this.ComType.InvokeMember(name, BindingFlags.GetProperty, null, this.ComObject, args);
        }
        public int Count
        {
            get { return (int)this._invoke("Count"); }
        }
        public object NewEnum
        {
            get { return this._invoke("_NewEnum"); }
        }
        public Match this[int index]
        {
            get { return new Match(this._invoke("Item", new object[] { index })); }
        }
        public IEnumerator<Match> GetEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }
    }
}
