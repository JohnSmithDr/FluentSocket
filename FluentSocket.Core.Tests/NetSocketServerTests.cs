using System;
using System.Threading;
using Xunit;
using FluentSocket;
using FluentSocket.Reactive;

namespace Tests
{
    public class NetSocketServerTests
    {
        const int ServerPort = 3000;

        [Fact]
        public void TestListen()
        {
            using (var listeningSignal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            {
                server.Listening += (s, e) => listeningSignal.Set();

                var run = server.ListenAsync(ServerPort);
                listeningSignal.WaitOne(2000);
                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(ServerPort.ToString(), server.LocalEndPoint.Port);
                server.Close();
            }
        }

        [Fact]
        public void TestOnListening()
        {
            using (var listeningSignal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            using (var subListening = server.OnListening().Subscribe(x => listeningSignal.Set()))
            {
                var run = server.ListenAsync(ServerPort);
                listeningSignal.WaitOne(2000);
                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(ServerPort.ToString(), server.LocalEndPoint.Port);
                server.Close();
            }
        }

        [Fact]
        public void TestClose() 
        {
            using (var listeningSignal = new AutoResetEvent(false))
            using (var closedSignal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            {
                server.Listening += (s, e) => listeningSignal.Set();
                server.Closed += (s, e) => closedSignal.Set();

                var run = server.ListenAsync(ServerPort);
                listeningSignal.WaitOne(2000);

                server.Close();
                closedSignal.WaitOne(2000);
            }
        }

        [Fact]
        public void TestOnClosed() 
        {
            using (var listeningSignal = new AutoResetEvent(false))
            using (var closedSignal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            using (var subListening = server.OnListening().Subscribe(x => listeningSignal.Set()))
            using (var subClosed = server.OnClosed().Subscribe(x => closedSignal.Set()))
            {
                var run = server.ListenAsync(ServerPort);
                listeningSignal.WaitOne(2000);

                server.Close();
                closedSignal.WaitOne(2000);
            }
        }
    }
}
