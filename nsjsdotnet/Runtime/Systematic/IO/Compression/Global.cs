namespace nsjsdotnet.Runtime.Systematic.IO.Compression
{
    using global::System;

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

            owner.Set("LZ77", LZ77.Global.GlobalTemplate);
        }
    }
}
