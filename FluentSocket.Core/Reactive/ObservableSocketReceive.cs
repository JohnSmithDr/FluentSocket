using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket.Reactive
{
    internal class ObservableSocketReceive : IObservable<Buffer>
    {
        private ISocket Socket { get; }
        private CancellationToken CancellationToken { get; }
        private Subject<Buffer> ReceiveSubject { get; set; }
        private Task ReceiveTask { get; set; }

        public ObservableSocketReceive(ISocket socket, CancellationToken cancellationToken)
        {
            Socket = socket;
            CancellationToken = cancellationToken;
        }
        
        public IDisposable Subscribe(IObserver<Buffer> observer)
        {
            if (ReceiveSubject == null) 
                ReceiveSubject = new Subject<Buffer>();

            if (ReceiveTask == null) 
            {
                ReceiveTask = CreateReceivingTask();
                ReceiveTask.ConfigureAwait(false);
            }
            else if (ReceiveTask.IsCompleted)
            {
                observer.OnCompleted();
            }

            return ReceiveSubject.Subscribe(observer);
        }

        private Task CreateReceivingTask()
        {
            return Task.Factory
                .StartNew(
                    ProcessReceivingAsync,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                )
                .Unwrap();
        }

        private async Task ProcessReceivingAsync()
        {
            try
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    var buff = new byte[Socket.ReceiveBufferSize];
                    using (var ns = Socket.GetInputStream())
                    {
                        var read = await ns.ReadAsync(buff, 0, buff.Length, CancellationToken);
                        if (read > 0)
                        {
                            var buffer = Buffer.FromBytes(buff, 0, read);
                            ReceiveSubject.OnNext(buffer);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ReceiveSubject.OnCompleted();
            }
            catch (Exception ex)
            {
                ReceiveSubject.OnError(ex);
                return;
            }
        }
    }
}