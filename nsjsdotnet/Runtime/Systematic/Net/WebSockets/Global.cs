namespace nsjsdotnet.Runtime.Systematic.Net.WebSockets
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
            owner.Set("WebSocketClient", WebSocketClient.ClassTemplate);
            owner.Set("WebSocketServer", WebSocketServer.ClassTemplate);
        }
    }
}
