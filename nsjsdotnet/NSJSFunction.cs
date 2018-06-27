namespace nsjsdotnet
{
    using nsjsdotnet.Core.Utilits;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public unsafe class NSJSFunction : NSJSValue
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_function_new(IntPtr isolate, IntPtr value);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_object_property_call(IntPtr isolate, IntPtr recv,
            IntPtr function, int argc, IntPtr* argv, ref NSJSStructural.NSJSExceptionInfo exception);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, int flNewProtect, out uint lpflOldProtect);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr IParam);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static double nsjs_localvalue_get_float64(IntPtr localValue);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool initialized;

        protected static internal NSJSFunction GetFrameworkFunction(NSJSVirtualMachine machine, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            NSJSFunction function = null;
            lock (machine)
            {
                function = machine.GetData<NSJSFunction>(key);
                if (function == null)
                {
                    NSJSObject o = machine.Global;
                    function = o.Get(key) as NSJSFunction;
                    if (function == null)
                    {
                        throw new InvalidProgramException("Framework system internal function appears to be deleted");
                    }
                    function.CrossThreading = true;
                    machine.SetData(key, function);
                }
            }
            return function;
        }

        public override object GetValue()
        {
            lock (this)
            {
                if (!this.initialized)
                {
                    this.initialized = true;
                    this.value = this.LocalValueToString();
                }
                return this.value;
            }
        }

        private const int PAGE_EXECUTE_READWRITE = 64;

        public static bool MarkShellcode(byte[] buffer, int ofs, int count)
        {
            if (buffer.Length == ofs)
            {
                return MarkShellcode(NULL, count);
            }
            fixed (byte* pinned = &buffer[ofs])
            {
                return MarkShellcode((IntPtr)pinned, count);
            }
        }

        public static bool MarkShellcode(IntPtr address, int count)
        {
            uint lpflOldProtect = 0;
            return VirtualProtect(address, count, PAGE_EXECUTE_READWRITE, out lpflOldProtect);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static byte[] shellcodeInfercurrentmethodaddress = { 85, 139, 229, 129, 236, 192, 0, 0, 0, 139, 229, 139, 229, 93, 195 };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static byte[] ofsInfermethodaddressVector = { 0x42, 0x62, 0x1E };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly static InferCurrentMethodAddressProc pfnInferCurrentMethodAddress;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr InferCurrentMethodAddressProc();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr InferCurrentObjectAddressProc();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSObject m_This;

        static NSJSFunction()
        {
            IntPtr address = GCHandle.Alloc(shellcodeInfercurrentmethodaddress, GCHandleType.Pinned).AddrOfPinnedObject();
            MarkShellcode(address, shellcodeInfercurrentmethodaddress.Length);
            pfnInferCurrentMethodAddress = FunctionPtrToDelegate<InferCurrentMethodAddressProc>(address);
        }

        public virtual NSJSObject This
        {
            get
            {
                return this.m_This;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_This = value;
                value.CrossThreading = true; // must
            }
        }

        internal NSJSFunction(IntPtr handle, NSJSObject owner, NSJSVirtualMachine machine) : base(handle, NSJSDataType.kFunction, machine)
        {
            if (owner == null)
            {
                owner = machine.Global;
            }
            this.This = owner;
        }

        public static IntPtr DelegateToFunctionPtr(Delegate d)
        {
            return MarshalAs.DelegateToFunctionPtr(d);
        }

        public static T FunctionPtrToDelegate<T>(IntPtr ptr)
        {
            return MarshalAs.FunctionPtrToDelegate<T>(ptr);
        }

        public static IntPtr Infer()
        {
            if (Environment.Is64BitProcess)
            {
                throw new PlatformNotSupportedException();
            }
            IntPtr radix = pfnInferCurrentMethodAddress();
            for (int i = 0; i < ofsInfermethodaddressVector.Length; i++)
            {
                IntPtr address = radix + ofsInfermethodaddressVector[i];
                if ((*(byte*)(address)) == 0xB8)
                {
                    return address;
                }
            }
            return NULL;
        }

        public static T Infer<T>()
        {
            IntPtr address = NSJSFunction.Infer();
            if (address == NULL)
            {
                return default(T);
            }
            return FunctionPtrToDelegate<T>(address);
        }

        public static NSJSFunction New(NSJSVirtualMachine machine, NSJSFunctionCallback value)
        {
            IntPtr address = NULL;
            if (value != null)
            {
                address = NSJSFunction.DelegateToFunctionPtr(value);
            }
            return InternalNew(machine, address);
        }

        public static NSJSFunction New(NSJSVirtualMachine machine, NSJSFunctionCallback2 value)
        {
            IntPtr address = NULL;
            if (value != null)
            {
                address = NSJSFunction.DelegateToFunctionPtr(value);
            }
            return InternalNew(machine, address);
        }

        private static NSJSFunction InternalNew(NSJSVirtualMachine machine, IntPtr value)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            if (value == NULL)
            {
                throw new ArgumentNullException("value");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            IntPtr handle = nsjs_localvalue_function_new(isolate, value);
            if (handle == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            return new NSJSFunction(handle, null, machine);
        }

        private TResult Call<TResult>(IEnumerable<NSJSValue> args, Func<List<IntPtr>, TResult> d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            List<IntPtr> argc = new List<IntPtr>(256);
            if (args != null)
            {
                foreach (NSJSValue value in args)
                {
                    NSJSValue i = value;
                    if (i == null)
                    {
                        i = NSJSValue.Null(this.VirtualMachine);
                    }
                    argc.Add(i.Handle);
                }
            }
            return d(argc);
        }

        public virtual NSJSValue Call(params NSJSValue[] args)
        {
            return this.Call((IEnumerable<NSJSValue>)args);
        }

        public override string ToString()
        {
            return this.GetValue().ToString();
        }

        public virtual NSJSValue Call(IEnumerable<NSJSValue> args)
        {
            NSJSValue result = this.Call(args, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual NSJSValue Call(NSJSValue[] args, out NSJSException exception)
        {
            return this.Call((IEnumerable<NSJSValue>)args, out exception);
        }

        public virtual NSJSValue Call(IEnumerable<NSJSValue> args, out NSJSException exception)
        {
            NSJSObject owner = this.m_This;
            if (owner == null)
            {
                throw new InvalidOperationException("this");
            }
            NSJSException exception_info = null;
            NSJSValue result = this.Call(args, (argc) =>
            {
                NSJSVirtualMachine machine = this.VirtualMachine;
                fixed (IntPtr* argv = argc.ToArray())
                {
                    IntPtr handle = nsjs_localvalue_object_property_call(machine.Isolate,
                        owner.Handle,
                        this.Handle,
                        argc.Count, argv,
                        ref *machine.exception);
                    exception_info = NSJSException.From(machine, machine.exception);
                    if (handle == NULL)
                    {
                        return null;
                    }
                    return NSJSValueBuilder.From(handle, this.VirtualMachine);
                }
            });
            exception = exception_info;
            return result;
        }

        public virtual NSJSValue Call(string[] args)
        {
            return this.Call((IEnumerable<string>)args);
        }

        public virtual NSJSValue Call(string[] args, out NSJSException exception)
        {
            return this.Call((IEnumerable<string>)args, out exception);
        }

        public virtual NSJSValue Call(IEnumerable<string> args)
        {
            NSJSValue result = this.Call(args, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual NSJSValue Call(IEnumerable<string> args, out NSJSException exception)
        {
            IList<NSJSValue> argv = new List<NSJSValue>();
            NSJSVirtualMachine machine = this.VirtualMachine;
            foreach (string s in args)
            {
                NSJSValue value = null;
                if (s == null)
                {
                    value = NSJSValue.Null(machine);
                }
                else
                {
                    value = NSJSString.New(machine, s);
                }
                if (value == null)
                {
                    continue;
                }
                argv.Add(value);
            }
            return this.Call(argv, out exception);
        }
    }
}
