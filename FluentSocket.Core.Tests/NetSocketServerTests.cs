using System;
using System.Threading;
using Xunit;
using FluentSocket;
using FluentSocket.Reactive;

namespace Tests
{
    public class NetSocketServerTests
    {
        const int Port = 3000;

        [Fact]
        public void TestServer()
        {
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

                var run = server.ListenAsync(Port);
                signal.WaitOne(2000);
                signal.Reset();

                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(Port.ToString(), server.LocalEndPoint.Port);

                clientSocket = NetSocketFactory.Default.ConnectAsync("127.0.0.1", Port).GetAwaiter().GetResult();
                signal.WaitOne(2000);
                signal.Reset();

                Assert.NotNull(connectedSocket);
                Assert.NotNull(connectedSocket.LocalEndPoint);
                Assert.NotNull(connectedSocket.RemoteEndPoint);
                Assert.NotNull(clientSocket);

                clientSocket.Dispose();
                connectedSocket.Dispose();

                server.Close();
                signal.WaitOne(2000);
                signal.Reset();
            }
        }

        [Fact]
        public void TestReactiveServer()
        {
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

                var run = server.ListenAsync(Port).ConfigureAwait(false);
                signal.WaitOne(2000);
                signal.Reset();
                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(Port.ToString(), server.LocalEndPoint.Port);

                clientSocket = NetSocketFactory.Default.ConnectAsync("127.0.0.1", Port).GetAwaiter().GetResult();
                signal.WaitOne(2000);
                signal.Reset();

                Assert.NotNull(connectedSocket);
                Assert.NotNull(connectedSocket.LocalEndPoint);
                Assert.NotNull(connectedSocket.RemoteEndPoint);
                Assert.NotNull(clientSocket);

                clientSocket.Dispose();
                connectedSocket.Dispose();
                subConnections.Dispose();

                server.Close();
                signal.WaitOne(2000);
                signal.Reset();
            }
        }
    }
}
