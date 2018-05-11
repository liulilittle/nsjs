namespace nsjsdotnet.Runtime.Systematic.Text
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

            owner.AddObject("Encoding", Encoding.ClassTemplate);
        }
    }
}
