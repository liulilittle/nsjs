namespace nsjsdotnet.Runtime.Systematic.Net
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
            owner.Set("Dns", Dns.ClassTemplate);
            owner.Set("HttpClient", HttpClient.ClassTemplate);
            owner.Set("HttpProxy", HttpProxy.ClassTemplate);
            owner.Set("Mail", Mail.ClassTemplate);
            owner.Set("Web", Web.Global.GlobalTemplate);
            owner.Set("Sockets", Sockets.Global.GlobalTemplate);
            owner.Set("WebSockets", WebSockets.Global.GlobalTemplate);
        }
    }
}
