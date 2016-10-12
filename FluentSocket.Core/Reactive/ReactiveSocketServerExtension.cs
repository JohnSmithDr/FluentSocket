using System;
using System.Reactive;
using System.Reactive.Linq;

namespace FluentSocket.Reactive
{
    public static class ReactiveSocketServerExtension
    {
        public static IObservable<Unit> OnListening(this ISocketServer server)
        {
            return Observable
                .FromEventPattern<EventHandler, EventArgs>(h => server.Listening += h, h => server.Listening -= h)
                .Select(x => Unit.Default);
        }

        public static IObservable<Unit> OnClosed(this ISocketServer server)
        {
            return Observable
                .FromEventPattern<EventHandler, EventArgs>(h => server.Closed += h, h => server.Closed -= h)
                .Select(x => Unit.Default);
        }

        public static IObservable<ISocket> OnConnections(this ISocketServer server)
        {
            return Observable
                .FromEventPattern<EventHandler<SocketConnectionEventArgs>, SocketConnectionEventArgs>(
                    h => server.Connected += h, h => server.Connected -= h)
                .Select(x => x.EventArgs.Socket);
        }
    }
}