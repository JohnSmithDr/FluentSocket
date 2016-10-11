using System;
using System.Threading.Tasks;

namespace FluentSocket
{
    public interface ISocketServer : IDisposable
    {
        event EventHandler Listening;
        event EventHandler Closed;
        event EventHandler<SocketConnectionEventArgs> Connected;

        NetEndPoint LocalEndPoint { get; }

        Task ListenAsync(int port);
        void Close();
    }
}