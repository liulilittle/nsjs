namespace nsjsdotnet.Core.Threading.Coroutines
{
    sealed class AbortCurrentCoroutine
    {
        private int g_exit = 0;

        public AbortCurrentCoroutine(int exit)
        {
            this.g_exit = exit;
        }

        public void Handle()
        {
            if (this.g_exit != 0)
            {
                throw new CoroutineAbortException();
            }
        }
    }
}
