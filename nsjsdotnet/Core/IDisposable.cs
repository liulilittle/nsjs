namespace nsjsdotnet.Core
{
    using System;

    public interface IDisposable : System.IDisposable
    {
        event EventHandler Disposed;

        bool IsDisposed { get; }
    }
}
