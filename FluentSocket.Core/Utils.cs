namespace FluentSocket
{
    static class Utils
    {
        public static void TryDispose(this System.IDisposable obj)
        {
            try { obj.Dispose(); }
            catch { }
        }
    }
}