namespace nsjsdotnet
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe static class NSJSJson
    {
        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_json_stringify(IntPtr isolate, IntPtr value, ref int len);

        [DllImport(NSJSStructural.NSJSVMLINKLIBRARY, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_json_parse(IntPtr isolate, byte* json, int jsonlen);

        private static readonly IntPtr NULL = IntPtr.Zero;

        public static NSJSValue Parse(NSJSVirtualMachine machine, string json)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }
            IntPtr isolate = machine.Isolate;
            if (isolate == NULL)
            {
                throw new InvalidOperationException("machine");
            }
            if (string.IsNullOrEmpty(json))
            {
                return NSJSValue.Null(machine);
            }
            IntPtr handle = NULL;
            byte[] cch = NSJSString.GetUTF8StringBuffer(json);
            fixed (byte* p = cch)
            {
                int count = cch.Length;
                if (count > 0 && cch[count - 1] == '\x0')
                {
                    count--;
                }
                handle = nsjs_localvalue_json_parse(isolate, p, count);
            }
            if (handle == NULL)
            {
                throw new ArgumentOutOfRangeException("json");
            }
            return NSJSValueBuilder.From(handle, machine);
        }

        public static string Stringify(NSJSValue value)
        {
            if (value == null)
            {
                return null;
            }
            int len = 0;
            IntPtr chunk = nsjs_localvalue_json_stringify(value.Isolate, value.Handle, ref len);
            if (chunk == NULL)
            {
                return null;
            }
            string result = chunk != NULL ? new string((sbyte*)chunk.ToPointer()) : null;
            NSJSMemoryManagement.Free(chunk);
            return result;
        }
    }
}
