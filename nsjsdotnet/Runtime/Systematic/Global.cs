namespace nsjsdotnet.Runtime.Systematic
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
            owner.Set("Data", Data.Global.GlobalTemplate);
            owner.Set("Text", Text.Global.GlobalTemplate);
            owner.Set("IO", IO.Global.GlobalTemplate);
            owner.Set("Net", Net.Global.GlobalTemplate);
            owner.Set("Configuration", Configuration.Global.GlobalTemplate);
            owner.Set("Cryptography", Cryptography.Global.GlobalTemplate);
            owner.Set("Sorting", Sorting.Global.GlobalTemplate);
            owner.Set("BitConverter", BitConverter.ClassTemplate);
        }
    }
}
