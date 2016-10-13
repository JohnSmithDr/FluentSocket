using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FluentSocket
{
    public class NetSocket : ISocket
    {
        public event EventHandler Closed;

        public Socket Socket { get; }
        public bool IsConnected { get; } = true;
        public int SendBufferSize => Socket.SendBufferSize;
        public int ReceiveBufferSize => Socket.ReceiveBufferSize;
        public NetEndPoint LocalEndPoint { get; }
        public NetEndPoint RemoteEndPoint { get; }

        public NetSocket (Socket socket)
        {
            Socket = socket;
            LocalEndPoint = Socket.LocalEndPoint.ToNetEndPoint();
            RemoteEndPoint = Socket.RemoteEndPoint.ToNetEndPoint();
        }
        
        public Stream GetInputStream() => new NetworkStream(Socket);
        public Stream GetOutputStream() => new NetworkStream(Socket);

        public ISocket SetSendBufferSize(int bufferSize)
        {
            Socket.SendBufferSize = bufferSize;
            return this;
        }

        public ISocket SetReceiveBufferSize(int bufferSize)
        {
            Socket.ReceiveBufferSize = bufferSize;
            return this;
        }

        public Task<int> SendAsync(byte[] buffer)
            => Socket.SendAsync(buffer, 0, buffer.Length);

        public Task<int> SendAsync(byte[] buffer, int offset, int count) 
            => Socket.SendAsync(buffer, offset, count);

        public Task<int> ReceiveAsync(byte[] buffer)
            => Socket.ReceiveAsync(buffer, 0, buffer.Length);
            
        public Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
            => Socket.ReceiveAsync(buffer, offset, count);

        public void Close()
        {
            try { Socket.Shutdown(SocketShutdown.Both); }
            catch { }
            Socket.TryDispose();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Close();
        }
    }
}