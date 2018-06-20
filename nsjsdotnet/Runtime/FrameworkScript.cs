namespace nsjsdotnet.Runtime
{
    using System;

    static class FrameworkScript
    {
        private static void ____nsjsdotnet_framework_object_defineproperty(NSJSVirtualMachine machine)
        {
            machine.Run(@"
                        function ____nsjsdotnet_framework_object_defineproperty(obj, key, get, set) {
                            'use strict'

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
                            'use strict'

                            if(obj === null || obj === undefined) {
                                return null;
                            }
                            if(typeof key !== 'string') {
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
                            'use strict'

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
                            'use strict'

                            if(obj === null || obj === undefined) {
                                return 0;
                            }
                            return (obj.hasOwnProperty(key) ? 1 : 0);
                        }");
        }

        private static void ____nsjsdotnet_framework_instanceof(NSJSVirtualMachine machine)
        {
            machine.Run(@"
                        function ____nsjsdotnet_framework_instanceof(instance, type) {
                            'use strict'

                            if (typeof type === 'string') {
                                type = eval(type);
                            }
                            if (typeof type !== 'function') {
                                return false;
                            }
                            var x1 = instance instanceof type;
                            if (x1) {
                                return true;
                            }
                            var x2 = false;
                            if (instance !== null && instance !== undefined) {
                                var __instproto__ = instance;
                                var __typesource = null;
                                do {
                                    __instproto__ = __instproto__.__proto__;
                                    if (__instproto__) {
                                        x2 = __instproto__.constructor === type;
                                        if (x2) {
                                            break;
                                        }
                                        if (__typesource === null) {
                                            __typesource = type.toString();
                                        }
                                        x2 = __typesource === __instproto__.constructor.toString();
                                        if (x2) {
                                            break;
                                        }
                                    }
                                } while (__instproto__);
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
