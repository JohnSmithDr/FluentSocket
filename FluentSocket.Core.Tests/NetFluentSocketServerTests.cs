using System;
using Xunit;
using FluentSocket;
using System.Threading;

namespace Tests
{
    public class NetFluentSocketServerTests
    {
        private ISocketServer Server { get; }
        private int ServerPort { get; } = 3000;

        public NetFluentSocketServerTests()
        {
            Server = NetSocketFactory.Default.CreateServer();
        }

        [Fact]
        public void TestListen()
        {
            Server.ListenAsync(ServerPort).ConfigureAwait(false);
            //Assert.Equal("0.0.0.0", Server.LocalEndPoint.Host);
            //Assert.Equal(ServerPort.ToString(), Server.LocalEndPoint.Port);
        }

        [Fact]
        public void TestClosed() 
        {
            var signal = new AutoResetEvent(false);

            Server.OnClosed().Subscribe(x => {
                signal.Set();
            });

            Server.Close();
            signal.WaitOne();
        }

        [Fact]
        public void TestDispose()
        {
            Server.Dispose();
        }
    }
}
