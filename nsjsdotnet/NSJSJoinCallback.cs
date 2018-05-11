namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NSJSJoinCallback(IntPtr sender, IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NSJSJoinCallback2([MarshalAs(UnmanagedType.CustomMarshaler,
        MarshalTypeRef = typeof(NSJSJoinCallbackInfoMarshalAsMashaler))]NSJSVirtualMachine machine, IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NSJSJoinCallback3([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef =
        typeof(NSJSJoinCallbackInfoMarshalAsMashaler), MarshalCookie = "0")]NSJSVirtualMachine machine,
     [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef =
        typeof(NSJSJoinCallbackInfoMarshalAsMashaler), MarshalCookie = "1")]object state);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public sealed class NSJSJoinCallbackInfoMarshalAsMashaler : ICustomMarshaler
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        private enum MarshalAsKind : int
        {
            kVirtualMachine = 0,
            kStateObject = 1,
        }

        private readonly MarshalAsKind kind = MarshalAsKind.kVirtualMachine;

        private NSJSJoinCallbackInfoMarshalAsMashaler(MarshalAsKind kind)
        {
            if (!Enum.IsDefined(typeof(MarshalAsKind), kind))
            {
                throw new ArgumentOutOfRangeException("kind");
            }
            this.kind = kind;
        }

        public static ICustomMarshaler GetInstance(string cookie)
        {
            int.TryParse(cookie, out int iKind);
            return new NSJSJoinCallbackInfoMarshalAsMashaler((MarshalAsKind)iKind);
        }

        public void CleanUpManagedData(object ManagedObj)
        {

        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            
        }

        public int GetNativeDataSize()
        {
            if (this.kind == MarshalAsKind.kVirtualMachine)
            {
                return Marshal.SizeOf(typeof(NSJSVirtualMachine));
            }
            if (this.kind == MarshalAsKind.kStateObject)
            {
                return Marshal.SizeOf(typeof(object));
            }
            return 0;
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if (ManagedObj == null)
            {
                return NULL;
            }
            if (this.kind == MarshalAsKind.kVirtualMachine)
            {
                NSJSVirtualMachine machine = ManagedObj as NSJSVirtualMachine;
                if (machine == null)
                {
                    return NULL;
                }
                return machine.Isolate;
            }
            else if (this.kind == MarshalAsKind.kStateObject)
            {
                return NSJSMarshalAsUtility.ObjectToIUnknown(ManagedObj);
            }
            return NULL;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == NULL)
            {
                return null;
            }
            if (this.kind == MarshalAsKind.kStateObject)
            {
                return NSJSMarshalAsUtility.IUnknownToObject(pNativeData);
            }
            if (this.kind == MarshalAsKind.kVirtualMachine)
            {
                return NSJSVirtualMachine.From(pNativeData);
            }
            return null;
        }
    }
}
