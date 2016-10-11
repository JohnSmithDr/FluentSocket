using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket
{
    public class NetFluentSocket : ISocket
    {
        private Task _receivingTask;
        private CancellationTokenSource _receivingCancellation;  
        private Subject<Buffer> _receivingSubject;
        private Subject<Unit> _closedSubject;
        private IDisposable _sendSubscription;

        public Socket Socket { get; }
        public NetEndPoint LocalEndPoint { get; }
        public NetEndPoint RemoteEndPoint { get; }

        public NetFluentSocket (Socket socket)
        {
            Socket = socket;
            LocalEndPoint = Socket.LocalEndPoint.ToNetEndPoint();
            RemoteEndPoint = Socket.RemoteEndPoint.ToNetEndPoint();
        }

        public Stream GetInputStream() => new NetworkStream(Socket);
        public Stream GetOutputStream() => new NetworkStream(Socket);

        public IObservable<Unit> OnClosed()
        {
            if (_closedSubject == null) 
                _closedSubject = new Subject<Unit>();
            return _closedSubject;
        }

        public IObservable<Buffer> BeginReceive(int bufferSize)
        {
            if (_receivingSubject != null) 
                throw new InvalidOperationException("Receive has already began");

            Socket.ReceiveBufferSize = bufferSize;
            _receivingSubject = new Subject<Buffer>();
            
            CreateReceivingTask().ConfigureAwait(false);

            return _receivingSubject;
        }

        public IDisposable BeginSend(IObservable<Buffer> buffers)
        {
            if (_sendSubscription != null) 
                throw new InvalidOperationException("Send has already began.");

            return _sendSubscription = buffers.Subscribe(
                buffer => Socket.SendAsync(buffer.GetBytes()).ConfigureAwait(false));
        }

        public Task<int> SendAsync(byte[] buffer)
            => Socket.SendAsync(buffer, 0, buffer.Length);

        public Task<int> SendAsync(byte[] buffer, int offset, int count) 
            => Socket.SendAsync(buffer, offset, count);

        public Task<int> ReceiveAsync(byte[] buffer)
            => Socket.ReceiveAsync(buffer, 0, buffer.Length);
            
        public Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
            => Socket.SendAsync(buffer, offset, count);

        public void Close()
        {
            try { Socket.Shutdown(SocketShutdown.Both); }
            catch { }
            Socket.TryDispose();
            _closedSubject?.OnNext(Unit.Default);
            _closedSubject?.OnCompleted();
        }

        public void Dispose()
        {
            Close();
            _sendSubscription?.TryDispose();
            _sendSubscription = null;

            _receivingSubject?.TryDispose();
            _receivingSubject = null;
            
            _closedSubject?.TryDispose();
            _closedSubject = null;
        }

        private Task CreateReceivingTask()
        {
            if (_receivingTask == null)
            {
                _receivingCancellation = new CancellationTokenSource();
                _receivingTask = Task.Factory
                    .StartNew(
                        async () => 
                        {
                            await ProcessReceivingAsync(_receivingSubject, _receivingCancellation.Token);
                            if (!Socket.Connected) _closedSubject.OnNext(Unit.Default);
                            // TODO: test socket connection and raise closed
                            _receivingSubject.OnCompleted();
                            _receivingCancellation.TryDispose();
                            _receivingCancellation = null;
                        },
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default
                    );
            }
            return _receivingTask;
        }

        private async Task ProcessReceivingAsync(Subject<Buffer> subject, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var ns = new NetworkStream(Socket))
                    {
                        var buff = new byte[Socket.ReceiveBufferSize];
                        var read = await ns.ReadAsync(buff, 0, buff.Length, cancellationToken);

                        if (read > 0)
                        {
                            var buffer = Buffer.FromBytes(buff, 0, read);
                            subject.OnNext(buffer);
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}