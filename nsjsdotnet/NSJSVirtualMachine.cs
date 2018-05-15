namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using nsjsdotnet.Core.IO;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using RuntimeLibrary = nsjsdotnet.Runtime.Global;
    using SystemLibrary = nsjsdotnet.Runtime.Systematic.Global;
    using FrameworkScript = nsjsdotnet.Runtime.FrameworkScript;
    using nsjsdotnet.Core.Utilits;

    public unsafe class NSJSVirtualMachine : EventArgs, IRelational
    {
        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        private extern static IntPtr nsjs_virtualmachine_new();

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_virtualmachine_add_c_extension(IntPtr machine);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_virtualmachine_free(IntPtr machine);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static sbyte* nsjs_virtualmachine_run(IntPtr machine, void* source,
            void* alias, ref NSJSStructural.NSJSExceptionInfo exception);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_virtualmachine_initialize(IntPtr machine);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_initialize(void* exce_path);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_uninitialize();

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool nsjs_virtualmachine_function_add([In]IntPtr machine,
            [MarshalAs(UnmanagedType.LPStr)]string function_name,
            IntPtr function_callback);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool nsjs_virtualmachine_function_remove([In]IntPtr machine,
            [MarshalAs(UnmanagedType.LPStr)]string function_name);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static sbyte* nsjs_virtualmachine_eval([In]IntPtr machine, void* expression, ref NSJSStructural.NSJSExceptionInfo exception);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static IntPtr nsjs_virtualmachine_call2([In]IntPtr machine, void* name, int argc, void** argv, ref NSJSStructural.NSJSExceptionInfo exception);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static sbyte* nsjs_virtualmachine_call([In]IntPtr machine, void* name, int argc, IntPtr* argv, ref NSJSStructural.NSJSExceptionInfo exception);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static IntPtr nsjs_virtualmachine_callvir([In]IntPtr machine, void* name, int argc, IntPtr* argv, ref NSJSStructural.NSJSExceptionInfo exception);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_virtualmachine_set_data([In]IntPtr machine, int solt, IntPtr data);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_virtualmachine_get_data([In]IntPtr machine, int solt);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_virtualmachine_get_isolate([In]IntPtr machine);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_virtualmachine_join([In]IntPtr machine, [In]IntPtr callback, IntPtr state);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_virtualmachine_object_add([In]IntPtr machine, void* function_name, IntPtr object_template);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static bool nsjs_virtualmachine_object_remove([In]IntPtr machine, void* function_name);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_virtualmachine_get_global([In]IntPtr machine);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_null(IntPtr isolate);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr nsjs_localvalue_undefined(IntPtr isolate);

        [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void nsjs_virtualmachine_abort(IntPtr machine);

        public sealed class ExtensionObjectTemplate : System.IDisposable
        {
            [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
            private extern static void nsjs_virtualmachine_object_free([In]IntPtr object_template);

            [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
            private extern static IntPtr nsjs_virtualmachine_object_new();

            [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
            private extern static bool nsjs_virtualmachine_object_addfunction([In]IntPtr object_template, IntPtr function_name, IntPtr callback);

            [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
            private extern static bool nsjs_virtualmachine_object_removefunction([In]IntPtr object_template, IntPtr function_name);

            [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
            private extern static bool nsjs_virtualmachine_object_addobject([In]IntPtr owner, IntPtr object_name, IntPtr object_template);

            [DllImport("nsjs.dll", CallingConvention = CallingConvention.Cdecl)]
            private extern static bool nsjs_virtualmachine_object_removeobject([In]IntPtr owner, IntPtr object_name);

            private IntPtr m_handle = NULL;
            private bool m_disposed = false;

            public IntPtr Handle
            {
                get
                {
                    return m_handle;
                }
            }

            public object Tag
            {
                get;
                set;
            }

            public ExtensionObjectTemplate()
            {
                this.m_handle = nsjs_virtualmachine_object_new();
                if (this.m_handle == NULL)
                {
                    throw new InvalidOperationException();
                }
            }

            public bool AddFunction(string name, NSJSFunctionCallback callback)
            {
                return InternalAddFunction(name, callback);
            }

            public bool AddFunction(string name, NSJSFunctionCallback2 callback)
            {
                return InternalAddFunction(name, callback);
            }

            private bool InternalAddFunction(string name, Delegate callback)
            {
                return Handling<bool>(name, (s) => nsjs_virtualmachine_object_addfunction(this.m_handle, s, NSJSFunction.DelegateToFunctionPtr(callback)));
            }

            public bool RemoveFunction(string name)
            {
                return Handling<bool>(name, (s) => nsjs_virtualmachine_object_removefunction(this.m_handle, s));
            }

            public bool AddObject(string name, ExtensionObjectTemplate object_template)
            {
                if (object_template == null)
                {
                    throw new ArgumentNullException("object_template");
                }
                return Handling<bool>(name, (s) => nsjs_virtualmachine_object_addobject(this.m_handle, s, object_template.m_handle));
            }

            public bool RemoveObject(string name)
            {
                return Handling<bool>(name, (s) => nsjs_virtualmachine_object_removeobject(this.m_handle, s));
            }

            private TResult Handling<TResult>(string name, Func<IntPtr, TResult> d)
            {
                if (d == null)
                {
                    throw new ArgumentNullException("d");
                }
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                if (name.Length <= 0)
                {
                    throw new ArgumentException("name");
                }
                fixed (byte* p = Encoding.UTF8.GetBytes(name))
                {
                    return d((IntPtr)p);
                }
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (!this.m_disposed)
                    {
                        this.m_disposed = true;
                        nsjs_virtualmachine_object_free(this.m_handle);
                    }
                }
            }
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.nullable = null;
                    this.undefined = null;
                    NSJSStructural.NSJSExceptionInfo.Free(this.exception);
                    NSJSStructural.NSJSStackTrace.Free(this.stacktrace);
                    this.stacktrace = null;
                    this.exception = null;
                    runings.Clear();
                    datas.Clear();
                    NSJSVirtualMachine machine;
                    machines.TryRemove(this.Handle, out machine);
                    this.OnDisposed(EventArgs.Empty);
                    cookie.Free();
                    nsjs_virtualmachine_free(this.Handle);
                }
            }
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDisposed(EventArgs e)
        {
            EventHandler evt = this.Disposed;
            if (evt != null)
            {
                evt(this, EventArgs.Empty);
            }
        }

        private static readonly IntPtr NULL = IntPtr.Zero;

        private static readonly ConcurrentDictionary<IntPtr, NSJSVirtualMachine> machines = new ConcurrentDictionary<IntPtr, NSJSVirtualMachine>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IntPtr isolate = NULL;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool disposed = false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool initialize = false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private GCHandle cookie;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSObject global = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSValue nullable = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NSJSValue undefined = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal NSJSStructural.NSJSExceptionInfo* exception = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal NSJSStructural.NSJSStackTrace* stacktrace = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IDictionary<string, RunContext> runings = new Dictionary<string, RunContext>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ConcurrentDictionary<string, object> datas = new ConcurrentDictionary<string, object>();

        private class RunContext
        {
            public string result;
            public string source;
            public string alias;
            public NSJSException exception;
        }

        public virtual event EventHandler<NSJSUnhandledExceptionEventArgs> UnhandledException = null;
        public virtual event EventHandler<NSJSMessage> Message = null;
        public virtual event EventHandler Disposed = null;

        public bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        public object Tag
        {
            get;
            set;
        }

        public bool CrossThreading
        {
            get;
            set;
        }

        public bool IntegerBoolean
        {
            get;
            set;
        }

        public bool AutomaticallyPrintException
        {
            get;
            set;
        }

        internal bool HasUnhandledExceptionHandler()
        {
            return this.UnhandledException != null;
        }

        protected virtual internal void OnUnhandledException(NSJSUnhandledExceptionEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            EventHandler<NSJSUnhandledExceptionEventArgs> on = this.UnhandledException;
            if (on != null)
            {
                on(this, e);
            }
        }

        public NSJSVirtualMachine()
        {
            this.AutomaticallyPrintException = true;
            this.IntegerBoolean = true;
            this.Handle = nsjs_virtualmachine_new();
            if (this.Handle == NULL)
            {
                throw new InvalidOperationException("this");
            }
            machines.TryAdd(this.Handle, this);
            this.cookie = GCHandle.Alloc(this);
            this.exception = NSJSStructural.NSJSExceptionInfo.New();
            this.stacktrace = NSJSStructural.NSJSStackTrace.New();
        }

        ~NSJSVirtualMachine()
        {
            this.Dispose();
        }

        public NSJSValue Null()
        {
            lock (this)
            {
                if (this.nullable == null)
                {
                    throw new InvalidOperationException("this");
                }
                return this.nullable;
            }
        }

        public NSJSValue Undefined()
        {
            lock (this)
            {
                if (this.undefined == null)
                {
                    throw new InvalidOperationException("this");
                }
                return this.undefined;
            }
        }

        static NSJSVirtualMachine()
        {
            fixed (byte* exce_path = Encoding.UTF8.GetBytes(Application.ExecutablePath))
            {
                nsjs_initialize(exce_path);
            }
        }

        protected virtual internal void OnMessage(NSJSMessage e)
        {
            if (e != null && this.Message != null)
            {
                this.Message(this, e);
            }
        }

        public virtual void Abort()
        {
            if (this.Isolate == NULL)
            {
                throw new InvalidOperationException("isolate");
            }
            nsjs_virtualmachine_abort(this.Handle);
        }

        public virtual bool AddFunction(string name, NSJSFunctionCallback callback)
        {
            return InternalAddFunction(name, callback);
        }

        public virtual bool AddFunction(string name, NSJSFunctionCallback2 callback)
        {
            return InternalAddFunction(name, callback);
        }

        private bool InternalAddFunction(string name, Delegate callback)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length <= 0)
            {
                throw new ArgumentException("name");
            }
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return nsjs_virtualmachine_function_add(this.Handle, name, NSJSFunction.DelegateToFunctionPtr(callback));
        }

        public virtual bool RemoveFunction(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length <= 0)
            {
                throw new ArgumentException("name");
            }
            return nsjs_virtualmachine_function_remove(this.Handle, name);
        }

        public virtual bool AddObject(string name, ExtensionObjectTemplate object_template)
        {
            if (object_template == null)
            {
                throw new ArgumentNullException("object_template");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length <= 0)
            {
                throw new ArgumentException("name");
            }
            fixed (byte* s = Encoding.UTF8.GetBytes(name))
            {
                return nsjs_virtualmachine_object_add(Handle, s, object_template.Handle);
            }
        }

        public virtual bool RemoveObject(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length <= 0)
            {
                throw new ArgumentException("name");
            }
            fixed (byte* s = Encoding.UTF8.GetBytes(name))
            {
                return nsjs_virtualmachine_object_remove(Handle, s);
            }
        }

        public virtual NSJSObject Global
        {
            get
            {
                lock (this)
                {
                    if (this.global == null)
                    {
                        if (this.Isolate == NULL)
                        {
                            throw new InvalidOperationException("isolate");
                        }
                        IntPtr handle = nsjs_virtualmachine_get_global(this.Isolate);
                        if (handle == NULL)
                        {
                            throw new InvalidOperationException("handle");
                        }
                        this.global = new NSJSObject(handle, this)
                        {
                            CrossThreading = true
                        };
                    }
                    return this.global;
                }
            }
        }

        public virtual string Run(string expression)
        {
            return Run(expression, null);
        }

        public virtual string Run(string expression, out NSJSException exception)
        {
            string result = Run(expression, null, out exception);
            exception?.Raise();
            return result;
        }

        public virtual string Run(string source, string alias)
        {
            string result = Run(source, alias, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual string Run(string source, string alias, out NSJSException exception)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            NSJSException exception_info = null;
            string result = null;
            try
            {
                lock (this)
                {
                    RunContext context = new RunContext();
                    if (this.runings.TryGetValue(source, out context))
                    {
                        result = context.result;
                        exception_info = context.exception;
                        return result;
                    }
                }
                result = this.Executing((() =>
                {
                    Encoding encoding = Encoding.UTF8;
                    byte[] alias_buffer = null;
                    if (alias != null)
                    {
                        alias_buffer = encoding.GetBytes(alias);
                    }
                    byte[] source_buffer = encoding.GetBytes(source);
                    fixed (byte* pstr_source = source_buffer)
                    {
                        fixed (byte* pstr_alias = alias_buffer)
                        {
                            sbyte* pstr_result = nsjs_virtualmachine_run(this.Handle,
                                pstr_source, pstr_alias, ref *this.exception);
                            string result_str = pstr_result != null ? new string(pstr_result) : null;
                            NSJSMemoryManagement.Free(pstr_result);
                            exception_info = NSJSException.From(this, this.exception);
                            return result_str;
                        }
                    }
                }));
                lock (this)
                {
                    RunContext context = new RunContext()
                    {
                        exception = exception_info,
                        result = result,
                        alias = alias,
                        source = source
                    };
                    this.runings.Add(source, context);
                }
                return result;
            }
            finally
            {
                exception = exception_info;
            }
        }

        public virtual string Run(FileInfo path)
        {
            string result = Run(path, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual string Run(FileInfo path, out NSJSException exception)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (!path.Exists)
            {
                throw new FileNotFoundException("path");
            }
            byte[] source = File.ReadAllBytes(path.FullName);
            return Run(FileAuxiliary.GetEncoding(source).GetString(source), path.FullName, out exception);
        }

        public virtual void Join(NSJSJoinCallback callback)
        {
            Join(callback, NULL);
        }

        public virtual void Join(NSJSJoinCallback callback, IntPtr state)
        {
            InternalJoin(callback, state);
        }

        public virtual void Join(NSJSJoinCallback2 callback, IntPtr state)
        {
            InternalJoin(callback, state);
        }

        public virtual void Join(NSJSJoinCallback3 callback, IntPtr state)
        {
            InternalJoin(callback, state);
        }

        private void InternalJoin(Delegate callback, IntPtr state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            nsjs_virtualmachine_join(this.Handle, NSJSFunction.DelegateToFunctionPtr(callback), state);
        }

        public virtual void Join(NSJSJoinCallback callback, object state)
        {
            InternalJoin(callback, state);
        }

        public virtual void Join(NSJSJoinCallback2 callback, object state)
        {
            InternalJoin(callback, state);
        }

        public virtual void Join(NSJSJoinCallback3 callback, object state)
        {
            InternalJoin(callback, state);
        }

        private void InternalJoin(Delegate callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            IntPtr ptr = MarshalAs.ObjectToIUnknown(state);
            nsjs_virtualmachine_join(this.Handle, NSJSFunction.DelegateToFunctionPtr(callback), ptr);
        }

        public virtual string Eval(string expression)
        {
            string result = this.Eval(expression, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual string Eval(string expression, out NSJSException exception)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("expression");
            }
            NSJSException exception_info = null;
            string result = null;
            this.Executing(() =>
            {
                byte[] ch = Encoding.UTF8.GetBytes(expression);
                fixed (byte* s = ch)
                {
                    sbyte* p = nsjs_virtualmachine_eval(this.Handle, s, ref *this.exception);
                    exception_info = NSJSException.From(this, this.exception);
                    result = p != null ? new string(p) : null;
                    NSJSMemoryManagement.Free(p);
                    return result;
                }
            });
            exception = exception_info;
            return result;
        }

        public virtual string Call(string name, params string[] args)
        {
            return this.Call(name, (IEnumerable<string>)args);
        }

        public virtual string Call(string name, IEnumerable<string> args)
        {
            string result = this.Call(name, args, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual string Call(string name, IEnumerable<string> args, out NSJSException exception)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length <= 0)
            {
                throw new ArgumentException("name");
            }
            NSJSException exception_info = null;
            string result = null;
            this.Executing<string>(() =>
            {
                IList<GCHandle?> cookies = new List<GCHandle?>(256);
                IList<IntPtr> arguments = new List<IntPtr>(256);
                Encoding encoding = Encoding.UTF8;
                cookies.Add(GCHandle.Alloc(encoding.GetBytes(name), GCHandleType.Pinned));
                int argc = 0;
                if (args != null)
                {
                    foreach (string s in args)
                    {
                        byte[] ch = null;
                        if (s != null)
                        {
                            ch = encoding.GetBytes(s);
                        }
                        if (s == null || ch.Length <= 0)
                        {
                            cookies.Add(null);
                            arguments.Add(NULL);
                        }
                        else
                        {
                            GCHandle cookie = GCHandle.Alloc(ch, GCHandleType.Pinned);
                            cookies.Add(cookie);
                            arguments.Add(cookie.AddrOfPinnedObject());
                        }
                        argc++;
                    }
                }
                void** argv = null;
                if (argc > 0)
                {
                    void** ppv = stackalloc void*[argc];
                    for (int i = 0; i < argc; i++)
                    {
                        ppv[i] = arguments[i].ToPointer();
                    }
                    argv = ppv;
                }
                IntPtr chunk = nsjs_virtualmachine_call2(this.Handle, cookies[0].Value.AddrOfPinnedObject().ToPointer(), argc, argv, ref *this.exception);
                exception_info = NSJSException.From(this, this.exception);
                result = chunk != NULL ? new string((sbyte*)chunk.ToPointer()) : null;
                foreach (GCHandle? cookie in cookies)
                {
                    if (cookie == null)
                    {
                        continue;
                    }
                    cookie.Value.Free();
                }
                return result;
            });
            exception = exception_info;
            return result;
        }

        public virtual string Call(string name, params NSJSValue[] args)
        {
            return this.Call(name, (IEnumerable<NSJSValue>)args);
        }

        public virtual string Call(string name, IEnumerable<NSJSValue> args)
        {
            string result = this.Call(name, args, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual string Call(string name, NSJSValue[] args, out NSJSException exception)
        {
            return this.Call(name, (IEnumerable<NSJSValue>)args, out exception);
        }

        private TResult InternalCall<TResult>(string name, IEnumerable<NSJSValue> args, Func<List<IntPtr>, TResult> d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length <= 0)
            {
                throw new ArgumentException("name");
            }
            List<IntPtr> argc = new List<IntPtr>(256);
            if (args != null)
            {
                foreach (NSJSValue value in args)
                {
                    NSJSValue i = value;
                    if (i == null)
                    {
                        i = NSJSValue.Null(this);
                    }
                    argc.Add(i.Handle);
                }
            }
            return d(argc);
        }

        public virtual string Call(string name, IEnumerable<NSJSValue> args, out NSJSException exception)
        {
            NSJSException exception_info = null;
            string result = null;
            this.InternalCall(name, args, (List<IntPtr> argc) =>
            {
                fixed (byte* key = Encoding.UTF8.GetBytes(name))
                {
                    fixed (IntPtr* argv = argc.ToArray())
                    {
                        sbyte* chunk = nsjs_virtualmachine_call(this.Handle, key, argc.Count, argv, ref *this.exception);
                        exception_info = NSJSException.From(this, this.exception);
                        result = chunk != null ? new string(chunk) : null;
                        NSJSMemoryManagement.Free(chunk);
                        return result;
                    }
                }
            });
            exception = exception_info;
            return result;
        }

        public virtual NSJSValue Callvir(string name, params NSJSValue[] args)
        {
            NSJSValue result = this.Callvir(name, args, out NSJSException exception);
            exception?.Raise();
            return result;
        }

        public virtual NSJSValue Callvir(string name, IEnumerable<NSJSValue> args, out NSJSException exception)
        {
            NSJSException exception_info = null;
            NSJSValue result = this.InternalCall(name, args, (List<IntPtr> argc) =>
            {
                fixed (byte* key = Encoding.UTF8.GetBytes(name))
                {
                    fixed (IntPtr* argv = argc.ToArray())
                    {
                        IntPtr handle = nsjs_virtualmachine_callvir(this.Handle, key, argc.Count, argv, ref *this.exception);
                        exception_info = NSJSException.From(this, this.exception);
                        if (handle == NULL)
                        {
                            return null;
                        }
                        return NSJSValueBuilder.From(handle, this);
                    }
                }
            });
            exception = exception_info;
            return result;
        }

        protected virtual void Executing(Action a)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }
            this.Executing<object>(() =>
            {
                a();
                return null;
            });
        }

        protected virtual T Executing<T>(Func<T> d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            lock (this)
            {
                if (!this.initialize)
                {
                    this.initialize = true;
                    this.InitializationLibrary();
                    nsjs_virtualmachine_initialize(this.Handle);
                    nsjs_virtualmachine_set_data(this.Handle, 0, (IntPtr)this.cookie);
                    this.undefined = new NSJSValue(nsjs_localvalue_null(this.Isolate), NSJSValueType.kUndefined, this);
                    this.nullable = new NSJSValue(nsjs_localvalue_null(this.Isolate), NSJSValueType.kNull, this);
                    this.nullable.CrossThreading = true;
                    this.undefined.CrossThreading = true;
                    this.InitializationFramework();
                }
            }
            return d();
        }

        protected virtual void InitializationLibrary()
        {
            RuntimeLibrary.Initialization(this);
            nsjs_virtualmachine_add_c_extension(this.Handle);
        }

        protected virtual void InitializationFramework()
        {
            FrameworkScript.Initialization(this);
        }

        public static ExtensionObjectTemplate GetSystemTemplate()
        {
            return SystemLibrary.GlobalTemplate;
        }

        public virtual void SetData(int solt, object value)
        {
            this.SetData(solt, MarshalAs.ObjectToIUnknown(value));
        }

        public virtual T GetData<T>(int solt)
        {
            IntPtr p = this.GetData(solt);
            if (typeof(T) == typeof(IntPtr))
            {
                return (T)(object)p;
            }
            object obj = MarshalAs.IUnknownToObject(p);
            if (obj == null || !typeof(T).IsInstanceOfType(obj))
            {
                return default(T);
            }
            return (T)obj;
        }

        public virtual void SetData(int solt, IntPtr value)
        {
            if (solt < 1)
            {
                throw new ArgumentOutOfRangeException("solt");
            }
            nsjs_virtualmachine_set_data(this.Handle, solt, value);
        }

        public IntPtr Isolate
        {
            get
            {
                lock (this)
                {
                    if (isolate == NULL)
                    {
                        isolate = nsjs_virtualmachine_get_isolate(this.Handle);
                    }
                    return isolate;
                }
            }
        }

        public virtual IntPtr GetData(int solt)
        {
            if (solt < 1)
            {
                throw new ArgumentOutOfRangeException("solt");
            }
            return nsjs_virtualmachine_get_data(Handle, solt);
        }

        public virtual bool SetData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            return datas.AddOrUpdate(key, value, (i, o) => value) == value;
        }

        public virtual T GetData<T>(string key)
        {
            object obj = this.GetData(key);
            if (obj == null || !typeof(T).IsInstanceOfType(obj))
            {
                return default(T);
            }
            return (T)obj;
        }

        public virtual object GetData(string key)
        {
            if (key == null)
            {
                return null;
            }
            object o;
            if (datas.TryGetValue(key, out o))
            {
                return o;
            }
            return null;
        }

        public static NSJSVirtualMachine From(IntPtr handle)
        {
            if (handle == NULL)
            {
                return null;
            }
            NSJSVirtualMachine machine;
            if (machines.TryGetValue(handle, out machine))
            {
                return machine;
            }
            return MarshalAs.CookieToObject(handle) as NSJSVirtualMachine;
        }
    }
}
