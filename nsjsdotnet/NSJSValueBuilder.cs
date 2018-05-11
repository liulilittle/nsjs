namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;

    static class NSJSValueBuilder
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static NSJSValueType nsjs_localvalue_get_typeid(IntPtr localValue);

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
            NSJSValueType datatype = nsjs_localvalue_get_typeid(handle);
            if (!((datatype & NSJSValueType.kUndefined) > 0 || (datatype & NSJSValueType.kNull) > 0))
            {
                if ((datatype & NSJSValueType.kString) > 0)
                {
                    return new NSJSString(handle, machine);
                }
                else if ((datatype & NSJSValueType.kInt32) > 0)
                {
                    return new NSJSInt32(handle, machine);
                }
                else if ((datatype & NSJSValueType.kUInt32) > 0)
                {
                    return new NSJSUInt32(handle, machine);
                }
                else if ((datatype & NSJSValueType.kBoolean) > 0)
                {
                    return new NSJSBoolean(handle, machine);
                }
                else if ((datatype & NSJSValueType.kDouble) > 0)
                {
                    return new NSJSDouble(handle, machine);
                }
                else if ((datatype & NSJSValueType.kFunction) > 0)
                {
                    return new NSJSFunction(handle, owner, machine);
                }
                else if ((datatype & NSJSValueType.kInt64) > 0)
                {
                    return new NSJSInt64(handle, machine);
                }
                else if ((datatype & NSJSValueType.kDateTime) > 0)
                {
                    return new NSJSDateTime(handle, machine);
                }
                else if ((datatype & NSJSValueType.kArray) > 0)
                {
                    return new NSJSArray(handle, machine);
                }
                else if ((datatype & NSJSValueType.kInt8Array) > 0)
                {
                    return new NSJSInt8Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kUInt8Array) > 0)
                {
                    return new NSJSUInt8Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kInt16Array) > 0)
                {
                    return new NSJSInt16Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kUInt16Array) > 0)
                {
                    return new NSJSUInt16Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kInt32Array) > 0)
                {
                    return new NSJSInt32Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kUInt32Array) > 0)
                {
                    return new NSJSUInt32Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kFloat32Array) > 0)
                {
                    return new NSJSFloat32Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kFloat64Array) > 0)
                {
                    return new NSJSFloat64Array(handle, machine);
                }
                else if ((datatype & NSJSValueType.kObject) > 0)
                {
                    return new NSJSObject(handle, machine);
                }
            }
            return new NSJSValue(handle, datatype, machine);
        }
    }
}
