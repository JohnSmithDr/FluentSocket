using System;
using System.Reactive;
using System.Reactive.Linq;

namespace FluentSocket
{
    public static class SocketExtensions
    {
        
        public static IDisposable BeginSend(this ISocket socket, IObservable<Buffer> buffers)
        {
            return buffers.Subscribe(
                buffer => socket.SendAsync(buffer.GetBytes()).ConfigureAwait(false));
        }

        public static IObservable<Buffer> BeginReceive(this ISocket socket)
        {
            throw new NotImplementedException();
        }

        public static IObservable<Unit> OnClosed(this ISocket socket)
        {
            return Observable
              .FromEvent<EventHandler, EventArgs>(h => socket.Closed += h, h => socket.Closed -= h)
              .Select(x => Unit.Default); 
        }
    }
}