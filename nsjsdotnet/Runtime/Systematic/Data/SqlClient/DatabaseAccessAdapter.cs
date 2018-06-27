namespace nsjsdotnet.Runtime.Systematic.Data.SqlClient
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.Data.Database;
    using System;

    sealed class DatabaseAccessAdapter
    {
        private static readonly NSJSFunctionCallback2 g_CloseProc = NSJSPinnedCollection.
            Pinned<NSJSFunctionCallback2>(Close);

        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static DatabaseAccessAdapter()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.
                ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.Set("New", NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(New));
            owner.Set("Invalid", NSJSPinnedCollection.Pinned<NSJSFunctionCallback2>(Invalid));
        }

        public static DatabaseAccessAdapter GetAdapter(NSJSObject value)
        {
            return NSJSKeyValueCollection.Get<DatabaseAccessAdapter>(value);
        }

        private static void Invalid(NSJSFunctionCallbackInfo arguments)
        {
            arguments.SetReturnValue(GetAdapter(arguments.This) == null);
        }

        public static NSJSValue New(NSJSVirtualMachine machine, MSSQLDatabaseAccessAdapter adapter)
        {
            if (machine == null)
            {
                return null;
            }
            if (adapter == null)
            {
                return NSJSValue.Null(machine);
            }
            NSJSObject self = NSJSObject.New(machine);
            self.UserToken = adapter;
            self.Set("Close", g_CloseProc);
            self.Set("Dispose", g_CloseProc);
            return self;
        }

        public static void Close(NSJSFunctionCallbackInfo arguments)
        {
            arguments.SetReturnValue(ObjectAuxiliary.RemoveInKeyValueCollection(arguments.This));
        }

        public static void New(NSJSFunctionCallbackInfo arguments)
        {
            try
            {
                string server = (arguments.Length > 0 ? arguments[0] as NSJSString : null)?.Value;
                string database = (arguments.Length > 1 ? arguments[1] as NSJSString : null)?.Value;
                string loginId = (arguments.Length > 2 ? arguments[2] as NSJSString : null)?.Value;
                string password = (arguments.Length > 3 ? arguments[3] as NSJSString : null)?.Value;
                arguments.SetReturnValue(New(arguments.VirtualMachine, new MSSQLDatabaseAccessAdapter(arguments.VirtualMachine,
                    server, database, loginId, password)));
            }
            catch (Exception exception)
            {
                Throwable.Exception(arguments.VirtualMachine, exception);
            }
        }
    }
}
