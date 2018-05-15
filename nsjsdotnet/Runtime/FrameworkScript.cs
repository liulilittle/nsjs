namespace nsjsdotnet.Runtime
{
    using System;

    static class FrameworkScript
    {
        private static void ____nsjsdotnet_framework_object_defineproperty(NSJSVirtualMachine machine)
        {
            machine.Run(@"
                        function ____nsjsdotnet_framework_object_defineproperty(obj, key, get, set) {
                            var sink = new Object();
                            if(set) {
                                sink['set'] = set;
                            }
                            if(get) {
                                sink['get'] = get;
                            }
                            Object.defineProperty(obj, key, sink);
                        }");
        }

        public static void Initialization(NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            ____nsjsdotnet_framework_object_defineproperty(machine);
        }
    }
}
