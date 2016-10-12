using System;
using System.Threading;
using Xunit;
using FluentSocket;
using FluentSocket.Reactive;

namespace Tests
{
    public class NetSocketServerTests
    {
        [Fact]
        public void TestServer()
        {
            int port = 3000;

            using (var listeningSignal = new AutoResetEvent(false))
            using (var closedSignal = new AutoResetEvent(false))
            using (var connectedSignal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            {
                ISocket connectedSocket = null;
                ISocket clientSocket = null;

                server.Listening += (s, e) => listeningSignal.Set();

                server.Closed += (s, e) => closedSignal.Set();

                server.Connected += (s, e) => {
                    connectedSocket = e.Socket;
                    connectedSignal.Set();
                };

                var run = server.ListenAsync(port).ConfigureAwait(false);
                listeningSignal.WaitOne(2000);
                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(port.ToString(), server.LocalEndPoint.Port);

                var asyncConn = NetSocketFactory.Default.ConnectAsync("127.0.0.1", port)
                    .ContinueWith(t => clientSocket = t.Result);
                connectedSignal.WaitOne(2000);

                Assert.NotNull(connectedSocket);
                Assert.NotNull(connectedSocket.LocalEndPoint);
                Assert.NotNull(connectedSocket.RemoteEndPoint);
                connectedSocket.Close();
                connectedSocket?.Dispose();

                clientSocket?.Close();
                clientSocket?.Dispose();

                server.Close();
                closedSignal.WaitOne(2000);
            }
        }

        [Fact]
        public void TestReactiveServer()
        {
            int port = 3001;

            using (var listeningSignal = new AutoResetEvent(false))
            using (var closedSignal = new AutoResetEvent(false))
            using (var connectedSignal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            using (var subListening = server.OnListening().Subscribe(x => listeningSignal.Set()))
            using (var subClosed = server.OnClosed().Subscribe(x => closedSignal.Set()))
            {
                ISocket connectedSocket = null;
                ISocket clientSocket = null;
                
                IDisposable subConnections = server.OnConnections().Subscribe(x => {
                    connectedSocket = x;
                    connectedSignal.Set();
                });

                var run = server.ListenAsync(port).ConfigureAwait(false);
                listeningSignal.WaitOne(2000);
                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(port.ToString(), server.LocalEndPoint.Port);

                var asyncConn = NetSocketFactory.Default.ConnectAsync("127.0.0.1", port)
                    .ContinueWith(t => clientSocket = t.Result);
                connectedSignal.WaitOne(2000);

                Assert.NotNull(connectedSocket);
                Assert.NotNull(connectedSocket.LocalEndPoint);
                Assert.NotNull(connectedSocket.RemoteEndPoint);
                connectedSocket.Close();
                connectedSocket?.Dispose();

                subConnections.Dispose();
                server.Close();
                closedSignal.WaitOne(2000);
            }
        }
    }
}
