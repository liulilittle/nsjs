namespace nsjsdotnet.Core.Utilits
{
    using System;
    using System.Runtime.InteropServices;

    public static class MarshalAs
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        public static object CookieToObject(IntPtr pNativeData)
        {
            try
            {
                GCHandle handle = GCHandle.FromIntPtr(pNativeData);
                return handle.Target;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static object IUnknownToObject(IntPtr pNativeData)
        {
            if (pNativeData == NULL)
            {
                return NULL;
            }
            try
            {
                return Marshal.GetObjectForIUnknown(pNativeData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IntPtr ObjectToIUnknown(object ManagedObj)
        {
            if (ManagedObj == null)
            {
                return NULL;
            }
            try
            {
                return Marshal.GetIUnknownForObject(ManagedObj);
            }
            catch (Exception)
            {
                return NULL;
            }
        }

        public static bool ReleaseCookie(IntPtr pNativeData)
        {
            if (pNativeData == NULL)
            {
                return false;
            }
            try
            {
                GCHandle handle = GCHandle.FromIntPtr(pNativeData);
                handle.Free();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IntPtr ObjectToCookie(object ManagedObj)
        {
            if (ManagedObj == null)
            {
                return NULL;
            }
            try
            {
                return (IntPtr)GCHandle.Alloc(ManagedObj);
            }
            catch (Exception)
            {
                return NULL;
            }
        }

        public static object ToObject(IntPtr pNativeData)
        {
            if (pNativeData == NULL)
            {
                return null;
            }
            object result = IUnknownToObject(pNativeData);
            if (result == null)
            {
                result = CookieToObject(pNativeData);
            }
            return result;
        }

        public static IntPtr ToNative(object ManagedObj)
        {
            if (ManagedObj == null)
            {
                return NULL;
            }
            IntPtr result = ObjectToIUnknown(ManagedObj);
            if (result == NULL)
            {
                result = ObjectToCookie(ManagedObj);
            }
            return result;
        }

        public static IntPtr DelegateToFunctionPtr(Delegate d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            return Marshal.GetFunctionPointerForDelegate(d);
        }

        public static T FunctionPtrToDelegate<T>(IntPtr ptr)
        {
            object d = null;
            if (ptr != NULL)
            {
                d = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
            }
            return (T)d;
        }
    }
}
