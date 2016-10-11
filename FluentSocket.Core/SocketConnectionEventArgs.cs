using System;

namespace FluentSocket
{
    public class SocketConnectionEventArgs : EventArgs
    {
        public ISocket Socket { get; }

        public SocketConnectionEventArgs (ISocket socket)
        {
            Socket = socket;
        }
    }
}