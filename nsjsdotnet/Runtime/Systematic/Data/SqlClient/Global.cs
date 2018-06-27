namespace nsjsdotnet.Runtime.Systematic.Data.SqlClient
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
            owner.Set("DatabaseAccessAdapter", DatabaseAccessAdapter.ClassTemplate);
        }
    }
}
