using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace FluentSocket.Reactive
{
    public static class SocketExtensions
    {
        public static IDisposable BeginSend(this ISocket socket, IObservable<Buffer> buffers)
            => BeginSend(socket, buffers, null);

        public static IDisposable BeginSend(this ISocket socket, IObservable<Buffer> buffers, Action<Exception> onSendError)
        {
            return buffers.Subscribe(
                async buffer => {
                    try 
                    {
                        await socket.SendAsync(buffer.GetBytes());
                    }
                    catch (Exception ex)
                    {
                        onSendError?.Invoke(ex);
                    }
                });
        }

        public static IObservable<Buffer> BeginReceive(this ISocket socket)
            => BeginReceive(socket, CancellationToken.None);

        public static IObservable<Buffer> BeginReceive(this ISocket socket, CancellationToken cancellationToken)
            => new ObservableSocketReceive(socket, cancellationToken);

        public static IObservable<Unit> OnClosed(this ISocket socket)
        {
            return Observable
                .FromEvent<EventHandler, EventArgs>(h => socket.Closed += h, h => socket.Closed -= h)
                .Select(x => Unit.Default); 
        }
    }
}