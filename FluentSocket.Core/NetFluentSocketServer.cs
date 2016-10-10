using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket
{
    public class NetFluentSocketServer : ISocketServer
    {
        private Subject<Unit> _closeSubject;
        private Subject<ISocket> _connectionSubject;
        private TcpListener _listener;

        public NetEndPoint LocalEndPoint { get; private set; }

        public IObservable<Unit> OnClosed()
        {
            if (_closeSubject == null)
                _closeSubject = new Subject<Unit>();
            return _closeSubject;
        }

        public IObservable<ISocket> OnConnection()
        {
            if (_connectionSubject == null)
                _connectionSubject = new Subject<ISocket>();
            return _connectionSubject;
        }

        public Task ListenAsync(int port)
        {
            var ep = new IPEndPoint(IPAddress.Any, port);
            _listener = new TcpListener(ep);
            _listener.Start();

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
            _listener?.Stop();
            _listener = null;
            _closeSubject.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            Close();

            _connectionSubject?.TryDispose();
            _connectionSubject = null;

            _closeSubject?.TryDispose();
            _closeSubject = null;
        }

        private async Task ListenSocketsAsync()
        {
            while (true)
            {
                try
                {
                    var socket = await _listener.AcceptSocketAsync();
                    var fluentSocket = new NetFluentSocket(socket);
                    _connectionSubject?.OnNext(fluentSocket);
                }
                catch
                {
                    return;
                }
            }
        }
    }
}