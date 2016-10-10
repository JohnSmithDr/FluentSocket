using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FluentSocket
{
    public class NetSocketFactory : ISocketFactory
    {
        private static readonly Lazy<NetSocketFactory> _default = 
            new Lazy<NetSocketFactory>(() => new NetSocketFactory(), true);

        public static NetSocketFactory Default => _default.Value;

        public async Task<ISocket> ConnectAsync(string host, int port)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(host, port);
            return new NetFluentSocket(socket);
        }

        public ISocketServer CreateServer() => new NetFluentSocketServer();
    }
}