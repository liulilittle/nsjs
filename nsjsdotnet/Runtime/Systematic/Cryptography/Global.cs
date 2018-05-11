namespace nsjsdotnet.Runtime.Systematic.Cryptography
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

            owner.AddObject("RC4", RC4.ClassTemplate);
            owner.AddObject("MD5", MD5.Current.ClassTemplate);
            owner.AddObject("SHA1", SHA1.Current.ClassTemplate);
            owner.AddObject("SHA256", SHA256.Current.ClassTemplate);
            owner.AddObject("SHA384", SHA384.Current.ClassTemplate);
            owner.AddObject("SHA512", SHA512.Current.ClassTemplate);

            owner.AddObject("AES", AES.Current.ClassTemplate);

            owner.AddObject("AES256CFB", AES256CFB.Current.ClassTemplate);
            owner.AddObject("AES256CBC", AES256CBC.Current.ClassTemplate);
            owner.AddObject("AES256CTS", AES256CTS.Current.ClassTemplate);
            owner.AddObject("AES256ECB", AES256ECB.Current.ClassTemplate);
            owner.AddObject("AES256OFB", AES256ECB.Current.ClassTemplate);

            owner.AddObject("AES192CBC", AES192CBC.Current.ClassTemplate);
            owner.AddObject("AES192CTS", AES192CTS.Current.ClassTemplate);
            owner.AddObject("AES192ECB", AES192ECB.Current.ClassTemplate);
            owner.AddObject("AES192CFB", AES192CFB.Current.ClassTemplate);
            owner.AddObject("AES192OFB", AES192OFB.Current.ClassTemplate);

            owner.AddObject("AES128CFB", AES192CBC.Current.ClassTemplate);
            owner.AddObject("AES128OFB", AES192CTS.Current.ClassTemplate);
            owner.AddObject("AES128CBC", AES192ECB.Current.ClassTemplate);
            owner.AddObject("AES128CTS", AES192CFB.Current.ClassTemplate);
            owner.AddObject("AES128ECB", AES192OFB.Current.ClassTemplate);
        }
    }
}
