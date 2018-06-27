namespace nsjsdotnet.Runtime.Systematic.Configuration
{
    sealed class Global
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

            owner.Set("Ini", Ini.ClassTemplate);;
        }
    }
}
