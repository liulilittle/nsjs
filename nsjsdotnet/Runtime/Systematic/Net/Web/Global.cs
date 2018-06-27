namespace nsjsdotnet.Runtime.Systematic.Net.Web
{
    using System;

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
            owner.Set("HttpApplication", HttpApplication.ClassTemplate);
        }
    }
}
