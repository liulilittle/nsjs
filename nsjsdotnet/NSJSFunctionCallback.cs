namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NSJSFunctionCallback(IntPtr info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NSJSFunctionCallback2([MarshalAs(UnmanagedType.CustomMarshaler,
        MarshalTypeRef = typeof(NSJSFunctionCallbackInfoMarshalAsMashaler))]NSJSFunctionCallbackInfo info);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public sealed class NSJSFunctionCallbackInfoMarshalAsMashaler : ICustomMarshaler
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        public NSJSFunctionCallbackInfoMarshalAsMashaler(string cookie)
        {

        }

        public void CleanUpManagedData(object managedObj)
        {

        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {

        }

        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(NSJSFunctionCallbackInfo));
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            NSJSFunctionCallbackInfo info = (NSJSFunctionCallbackInfo)ManagedObj;
            if (info == null)
            {
                return NULL;
            }
            return info.Handle;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            return NSJSFunctionCallbackInfo.From(pNativeData);
        }

        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new NSJSFunctionCallbackInfoMarshalAsMashaler(cookie);
        }
    }
}
