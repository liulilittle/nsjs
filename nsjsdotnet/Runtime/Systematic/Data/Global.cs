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
            owner.Set("SqlClient", SqlClient.Global.GlobalTemplate);
            owner.Set("DataTableGateway", DataTableGateway.ClassTemplate);
        }
    }
}
