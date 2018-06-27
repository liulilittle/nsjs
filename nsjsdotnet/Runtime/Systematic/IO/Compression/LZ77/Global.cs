namespace nsjsdotnet.Runtime.Systematic.IO.Compression.LZ77
{
    static class Global
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate GlobalTemplate
        {
            get;
            private set;
        }

        static Global()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            GlobalTemplate = owner;

            owner.Set("GZip", GZip.Current.ClassTemplate);
            owner.Set("Deflate", Deflate.Current.ClassTemplate);
        }
    }
}
