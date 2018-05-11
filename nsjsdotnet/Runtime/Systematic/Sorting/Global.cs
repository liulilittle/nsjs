namespace nsjsdotnet.Runtime.Systematic.Sorting
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
            owner.AddObject("Sortable", Sortable.ClassTemplate);
        }
    }
}
