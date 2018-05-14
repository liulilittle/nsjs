namespace nsjsdotnet.Core
{
    using System;
    using System.IO;
    using System.Net.Sockets;

    public static class Throwable
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        public static void SocketException(this NSJSVirtualMachine machine)
        {
            Exception<SocketException>(machine);
        }

        public static void NullReferenceException(this NSJSVirtualMachine machine)
        {
            Exception<NullReferenceException>(machine, NSJSErrorKind.kReferenceError);
        }

        public static void ObjectDisposedException(this NSJSVirtualMachine machine)
        {
            Exception<ObjectDisposedException>(machine, NSJSErrorKind.kReferenceError);
        }

        public static void ArgumentNullException(this NSJSVirtualMachine machine)
        {
            Exception<ArgumentNullException>(machine);
        }

        public static void ArgumentException(this NSJSVirtualMachine machine)
        {
            Exception<ArgumentException>(machine);
        }

        public static void FileNotFoundException(this NSJSVirtualMachine machine)
        {
            Exception<FileNotFoundException>(machine);
        }

        public static void PlatformNotSupportedException(this NSJSVirtualMachine machine)
        {
            Exception<PlatformNotSupportedException>(machine);
        }

        public static void IOException(this NSJSVirtualMachine machine)
        {
            Exception<IOException>(machine);
        }

        public static void DirectoryNotFoundException(this NSJSVirtualMachine machine)
        {
            Exception<DirectoryNotFoundException>(machine);
        }

        public static void InvalidOperationException(this NSJSVirtualMachine machine)
        {
            Exception<InvalidOperationException>(machine);
        }

        public static void ArgumentOutOfRangeException(this NSJSVirtualMachine machine)
        {
            Exception<ArgumentOutOfRangeException>(machine);
        }

        public static void IndexOutOfRangeException(this NSJSVirtualMachine machine)
        {
            Exception<IndexOutOfRangeException>(machine);
        }

        public static void NotImplementedException(this NSJSVirtualMachine machine)
        {
            Exception<NotImplementedException>(machine);
        }

        public static void RankException(this NSJSVirtualMachine machine)
        {
            Exception<RankException>(machine);
        }

        public static void TimeoutException(this NSJSVirtualMachine machine)
        {
            Exception<TimeoutException>(machine);
        }

        public static void SystemException(this NSJSVirtualMachine machine)
        {
            Exception<SystemException>(machine);
        }

        public static void Exception<T>(this NSJSVirtualMachine machine)
            where T : Exception
        {
            Exception<T>(machine, NSJSErrorKind.kError);
        }

        public static void Exception<T>(this NSJSVirtualMachine machine, NSJSErrorKind kind)
            where T : Exception
        {
            NSJSException.Throw(machine, typeof(T).FullName, kind);
        }

        public static string FormatMessage(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }
            Type clazz = exception.GetType();
            return string.Format("{0} * {1}", clazz.FullName, exception.Message);
        }

        public static void Exception(this NSJSVirtualMachine machine, Exception exception)
        {
            do
            {
                if (machine == null || exception == null)
                {
                    break;
                }
                NSJSException.Throw(machine, exception);
            } while (false);
        }

        public static void NotSupportedException(this NSJSVirtualMachine machine)
        {
            NSJSException.Throw(machine, typeof(NotSupportedException).FullName,
                NSJSErrorKind.kError);
        }

        public static void ArgumentNullException(params object[] s)
        {
            const string STRSOLT = "solt";
            if (s != null)
            {
                int solt = 0;
                foreach (object i in s)
                {
                    if (NULL.Equals(i))
                    {
                        throw new ArgumentNullException(STRSOLT + solt);
                    }
                    if (i == null)
                    {
                        throw new ArgumentNullException(STRSOLT + solt);
                    }
                    solt++;
                }
            }
        }
    }
}
