using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;

namespace FluentSocket
{
    public interface ISocket : IDisposable
    {
        NetEndPoint LocalEndPoint { get; }
        NetEndPoint RemoteEndPoint { get; }

        Stream GetInputStream();
        Stream GetOutputStream();
        IObservable<Unit> OnClosed();
        IObservable<Buffer> BeginReceive(int bufferSize);
        IDisposable BeginSend(IObservable<Buffer> buffers);
        Task<int> SendAsync(byte[] buffer);
        Task<int> SendAsync(byte[] buffer, int offset, int count);
        Task<int> ReceiveAsync(byte[] buffer);
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
        void Close();
    }
}
