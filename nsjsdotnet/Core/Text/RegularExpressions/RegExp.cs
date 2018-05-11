namespace nsjsdotnet.Core.Text.RegularExpressions
{
    using System;
    using System.Reflection;

    public class RegExp
    {
        object ComObject; Type ComType;
        void Inint()
        {
            this.ComType = Type.GetTypeFromProgID("VBScript.RegExp");
            this.ComObject = Activator.CreateInstance(this.ComType);
        }
        public RegExp()
        {
            this.Inint();
        }
        public RegExp(string pattern, bool global = true, bool ignoreCase = false, bool multiline = true)
        {
            this.Inint();
            this.Pattern = pattern; this.Global = global;
            this.IgnoreCase = ignoreCase; this.Multiline = multiline;
        }
        object DoMethod(object target, Type type, string name, object[] args, BindingFlags invokeAttr)
        {
            return type.InvokeMember(name, invokeAttr, null, target, args);
        }
        object DoMethod(string name, object[] args, BindingFlags invokeAttr)
        {
            return this.DoMethod(this.ComObject, this.ComType, name, args, invokeAttr);
        }
        object _invoke(string name, object[] args = null)
        {
            return this.DoMethod(this.ComObject, this.ComType, name, args, BindingFlags.InvokeMethod);
        }
        object _invoke(string name, object args)
        {
            return this._invoke(name, new object[] { args });
        }
        object _set(string name, object args)
        {
            return this.DoMethod(this.ComObject, this.ComType, name, new object[] { args }, BindingFlags.SetProperty);
        }
        object _get(string name)
        {
            return this.DoMethod(this.ComObject, this.ComType, name, null, BindingFlags.GetProperty);
        }
        public bool IgnoreCase
        {
            get { return (bool)this._get("IgnoreCase"); }
            set { this._set("IgnoreCase", value); }
        }
        public bool Global
        {
            get { return (bool)this._get("Global"); }
            set { this._set("Global", value); }
        }
        public string Pattern
        {
            get { return this._get("Pattern").ToString(); }
            set { this._set("Pattern", value); }
        }
        public bool Multiline
        {
            get { return (bool)this._get("Multiline"); }
            set { this._set("Multiline", value); }
        }
        public bool Test(string sourceString)
        {
            return (bool)this._invoke("Test", sourceString);
        }
        public MatchCollection Execute(string sourceString)
        {
            return new MatchCollection(this._invoke("Execute", sourceString));
        }
        public string Replace(string sourceString, string replaceVar)
        {
            return this._invoke("Replace", new object[] { sourceString, replaceVar }).ToString();
        }
    }
}
