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

            using (var signal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            {
                ISocket connectedSocket = null;
                ISocket clientSocket = null;

                server.Listening += (s, e) => signal.Set();
                server.Closed += (s, e) => signal.Set();

                server.Connected += (s, e) => {
                    connectedSocket = e.Socket;
                    signal.Set();
                };

                var run = server.ListenAsync(port).ConfigureAwait(false);
                signal.WaitOne(2000);
                signal.Reset();

                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(port.ToString(), server.LocalEndPoint.Port);

                var asyncConn = NetSocketFactory.Default.ConnectAsync("127.0.0.1", port)
                    .ContinueWith(t => clientSocket = t.Result);
                signal.WaitOne(2000);
                signal.Reset();

                Assert.NotNull(connectedSocket);
                Assert.NotNull(connectedSocket.LocalEndPoint);
                Assert.NotNull(connectedSocket.RemoteEndPoint);
                connectedSocket.Close();
                connectedSocket?.Dispose();

                clientSocket?.Close();
                clientSocket?.Dispose();

                server.Close();
                signal.WaitOne(2000);
                signal.Reset();
            }
        }

        [Fact]
        public void TestReactiveServer()
        {
            int port = 3001;

            using (var signal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            using (var subListening = server.OnListening().Subscribe(x => signal.Set()))
            using (var subClosed = server.OnClosed().Subscribe(x => signal.Set()))
            {
                ISocket connectedSocket = null;
                ISocket clientSocket = null;
                
                IDisposable subConnections = server.OnConnections().Subscribe(x => {
                    connectedSocket = x;
                    signal.Set();
                });

                var run = server.ListenAsync(port).ConfigureAwait(false);
                signal.WaitOne(2000);
                signal.Reset();
                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(port.ToString(), server.LocalEndPoint.Port);

                var asyncConn = NetSocketFactory.Default.ConnectAsync("127.0.0.1", port)
                    .ContinueWith(t => clientSocket = t.Result);
                signal.WaitOne(2000);
                signal.Reset();

                Assert.NotNull(connectedSocket);
                Assert.NotNull(connectedSocket.LocalEndPoint);
                Assert.NotNull(connectedSocket.RemoteEndPoint);
                connectedSocket.Close();
                connectedSocket.Dispose();

                clientSocket?.Close();
                clientSocket?.Dispose();

                subConnections.Dispose();
                server.Close();
                signal.WaitOne(2000);
                signal.Reset();
            }
        }
    }
}
