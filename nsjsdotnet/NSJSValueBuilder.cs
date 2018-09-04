namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;

    [TypeLibType(TypeLibTypeFlags.FRestricted | TypeLibTypeFlags.FHidden)]
    internal static class NSJSValueBuilder
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static NSJSDataType nsjs_localvalue_get_typeid(IntPtr localValue);

        private static readonly IntPtr NULL = IntPtr.Zero;

        public static NSJSValue From(IntPtr handle, NSJSVirtualMachine machine)
        {
            return From(handle, null, machine);
        }

        public static NSJSValue From(IntPtr handle, NSJSObject owner, NSJSVirtualMachine machine)
        {
            if (machine == null)
            {
                return null;
            }
            if (handle == NULL)
            {
                return null;
            }
            NSJSDataType datatype = nsjs_localvalue_get_typeid(handle);
            if (!((datatype & NSJSDataType.kUndefined) > 0 || (datatype & NSJSDataType.kNull) > 0))
            {
                if ((datatype & NSJSDataType.kString) > 0)
                {
                    return new NSJSString(handle, machine);
                }
                else if ((datatype & NSJSDataType.kInt32) > 0)
                {
                    return new NSJSInt32(handle, machine);
                }
                else if ((datatype & NSJSDataType.kUInt32) > 0)
                {
                    return new NSJSUInt32(handle, machine);
                }
                else if ((datatype & NSJSDataType.kBoolean) > 0)
                {
                    return new NSJSBoolean(handle, machine);
                }
                else if ((datatype & NSJSDataType.kDouble) > 0)
                {
                    return new NSJSDouble(handle, machine);
                }
                else if ((datatype & NSJSDataType.kFunction) > 0)
                {
                    return new NSJSFunction(handle, owner, machine);
                }
                else if ((datatype & NSJSDataType.kInt64) > 0)
                {
                    return new NSJSInt64(handle, machine);
                }
                else if ((datatype & NSJSDataType.kDateTime) > 0)
                {
                    return new NSJSDateTime(handle, machine);
                }
                else if ((datatype & NSJSDataType.kArray) > 0)
                {
                    return new NSJSArray(handle, machine);
                }
                else if ((datatype & NSJSDataType.kInt8Array) > 0)
                {
                    return new NSJSInt8Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kUInt8Array) > 0)
                {
                    return new NSJSUInt8Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kInt16Array) > 0)
                {
                    return new NSJSInt16Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kUInt16Array) > 0)
                {
                    return new NSJSUInt16Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kInt32Array) > 0)
                {
                    return new NSJSInt32Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kUInt32Array) > 0)
                {
                    return new NSJSUInt32Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kFloat32Array) > 0)
                {
                    return new NSJSFloat32Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kFloat64Array) > 0)
                {
                    return new NSJSFloat64Array(handle, machine);
                }
                else if ((datatype & NSJSDataType.kObject) > 0)
                {
                    return new NSJSObject(handle, machine);
                }
            }
            return new NSJSValue(handle, datatype, machine);
        }
    }
}
