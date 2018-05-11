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
            owner.AddObject("Data", Data.Global.GlobalTemplate);
            owner.AddObject("Text", Text.Global.GlobalTemplate);
            owner.AddObject("IO", IO.Global.GlobalTemplate);
            owner.AddObject("Net", Net.Global.GlobalTemplate);
            owner.AddObject("Configuration", Configuration.Global.GlobalTemplate);
            owner.AddObject("Cryptography", Cryptography.Global.GlobalTemplate);
            owner.AddObject("Sorting", Sorting.Global.GlobalTemplate);
            owner.AddObject("BitConverter", BitConverter.ClassTemplate);
        }
    }
}
