using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket
{
    public class NetSocketServer : ISocketServer
    {
        private TcpListener _listener;

        public event EventHandler Listening;
        public event EventHandler Closed;
        public event EventHandler<SocketConnectionEventArgs> Connected;

        public NetEndPoint LocalEndPoint { get; private set; }

        public Task ListenAsync(int port)
        {
            var ep = new IPEndPoint(IPAddress.Any, port);
            _listener = new TcpListener(ep);
            _listener.Start();

            Listening?.Invoke(this, EventArgs.Empty);

            return Task.Factory
                .StartNew(
                    ListenSocketsAsync, 
                    CancellationToken.None, 
                    TaskCreationOptions.LongRunning, 
                    TaskScheduler.Default)
                .Unwrap();
        }

        public void Close()
        {
            if (_listener != null) 
            {
                _listener?.Stop();
                _listener = null;
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            Close();
        }

        private async Task ListenSocketsAsync()
        {
            while (true)
            {
                try
                {
                    var socket = await _listener.AcceptSocketAsync();
                    var newSocket = new NetSocket(socket);
                    Connected?.Invoke(this, new SocketConnectionEventArgs(newSocket));
                }
                catch
                {
                    // close tcp listener may cause exception
                    // TODO: check tcp listener state and raise closed event here
                    return;
                }
            }
        }
    }
}