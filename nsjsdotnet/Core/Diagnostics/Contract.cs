namespace nsjsdotnet.Core.Diagnostics
{
    using System;
    using System.Security;

    public static class Contract
    {
        //[Conditional("CONTRACTS_FULL")]
        public static void Requires<T>(bool condition) where T : Exception, new()
        {
            Contract.Requires<T>(condition, null);
        }

        //[Conditional("CONTRACTS_FULL")]
        public static void Requires<T>(bool condition, string message) where T : Exception, new()
        {
            if (!condition)
                Contract.Throw<T>(message);
        }

        [SecuritySafeCritical]
        private static void Throw<T>(string message) where T : Exception, new()
        {
            if (string.IsNullOrEmpty(message))
                throw new T();
            else
            {
                var ctor = typeof(T).GetConstructor(new Type[] { typeof(string) });
                throw ((T)ctor.Invoke(new object[] { message }));
            }
        }

        //[Conditional("CONTRACTS_FULL")]
        public static void Ensures<T>(bool condition) where T : Exception, new()
        {
            Contract.Requires<T>(condition);
        }

        //[Conditional("CONTRACTS_FULL")]
        public static void Ensures<T>(bool condition, string message) where T : Exception, new()
        {
            Contract.Requires<T>(condition, message);
        }

        //[Conditional("CONTRACTS_FULL")]
        public static void Invariant<T>(bool condition) where T : Exception, new()
        {
            Contract.Requires<T>(condition);
        }

        //[Conditional("CONTRACTS_FULL")]
        public static void Invariant<T>(bool condition, string message) where T : Exception, new()
        {
            Contract.Requires<T>(condition, message);
        }
    }
}
