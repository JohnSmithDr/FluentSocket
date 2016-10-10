using System;
using System.Reactive;
using System.Threading.Tasks;

namespace FluentSocket
{
    public interface ISocketServer : IDisposable
    {
        NetEndPoint LocalEndPoint { get; }

        IObservable<Unit> OnClosed();
        IObservable<ISocket> OnConnection();
        Task ListenAsync(int port);
        void Close();
    }
}