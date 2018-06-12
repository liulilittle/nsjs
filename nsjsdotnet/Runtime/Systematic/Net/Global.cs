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
            owner.AddObject("Dns", Dns.ClassTemplate);
            owner.AddObject("HttpClient", HttpClient.ClassTemplate);
            owner.AddObject("HttpProxy", HttpProxy.ClassTemplate);
            owner.AddObject("Mail", Mail.ClassTemplate);
            owner.AddObject("Web", Web.Global.GlobalTemplate);
            owner.AddObject("Sockets", Sockets.Global.GlobalTemplate);
            owner.AddObject("WebSockets", WebSockets.Global.GlobalTemplate);
        }
    }
}
