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

        private static void ____nsjsdotnet_framework_object_getpropertydescriptor(NSJSVirtualMachine machine)
        {
            machine.Run(@"
                        function ____nsjsdotnet_framework_object_getpropertydescriptor(obj, key) {
                            if(obj === null || obj === undefined) {
                                return null;
                            }
                            var result = Object.getOwnPropertyDescriptor(obj, key);
                            if(!result) {
                                return null;
                            }
                            return result;
                        }");
        }

        private static void ____nsjsdotnet_framework_object_getpropertynames(NSJSVirtualMachine machine)
        {
            machine.Run(@"
                        function ____nsjsdotnet_framework_object_getpropertynames(obj) {
                            if(obj === null || obj === undefined) {
                                return new Array();
                            }
                            return Object.getOwnPropertyNames(obj);
                        }");
        }

        private static void ____nsjsdotnet_framework_object_isdefined(NSJSVirtualMachine machine)
        {
            machine.Run(@"
                        function ____nsjsdotnet_framework_object_isdefined(obj, key) {
                            if(obj === null || obj === undefined) {
                                return 0;
                            }
                            return obj.hasOwnProperty(key) ? 1 : 0;
                        }");
        }

        private static void ____nsjsdotnet_framework_instanceof(NSJSVirtualMachine machine)
        {
            machine.Run(@"
                        var ____nsjsdotnet_framework_instanceof = function(instance, type) {
                            var x1 = instance instanceof type;
                            var x2 = false;
                            if (instance !== null && instance !== undefined) {
                                x2 = instance.constructor.toString() == type.toString();
                            }
                            return (x1 || x2) ? 1 : 0;
                        }");
        }

        public static void Initialization(NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            ____nsjsdotnet_framework_instanceof(machine);
            ____nsjsdotnet_framework_object_getpropertynames(machine);
            ____nsjsdotnet_framework_object_defineproperty(machine);
            ____nsjsdotnet_framework_object_isdefined(machine);
            ____nsjsdotnet_framework_object_getpropertydescriptor(machine);
        }
    }
}
