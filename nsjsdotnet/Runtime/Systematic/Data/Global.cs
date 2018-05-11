namespace nsjsdotnet.Runtime.Systematic.Data
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
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.
                ExtensionObjectTemplate();
            GlobalTemplate = owner;
            owner.AddObject("SqlClient", SqlClient.Global.GlobalTemplate);
            owner.AddObject("DataTableGateway", DataTableGateway.ClassTemplate);
        }
    }
}
