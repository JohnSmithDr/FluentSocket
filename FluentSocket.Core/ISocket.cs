using System;
using System.IO;
using System.Threading.Tasks;

namespace FluentSocket
{
    public interface ISocket : IDisposable
    {
        event EventHandler Closed;

        bool IsConnected { get; }
        int SendBufferSize { get; }
        int ReceiveBufferSize { get; }
        NetEndPoint LocalEndPoint { get; }
        NetEndPoint RemoteEndPoint { get; }

        ISocket SetSendBufferSize(int bufferSize);
        ISocket SetReceiveBufferSize(int bufferSize);
        Stream GetInputStream();
        Stream GetOutputStream();
        Task<int> SendAsync(byte[] buffer);
        Task<int> SendAsync(byte[] buffer, int offset, int count);
        Task<int> ReceiveAsync(byte[] buffer);
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
        void Close();
    }
}
