using Xunit;
using FluentSocket;
using System.Threading.Tasks;
using System.Threading;

namespace Tests
{
    public class NetSocketServerTests
    {
        const int ServerPort = 3000;

        [Fact]
        public void TestListen()
        {
            using (var signal = new AutoResetEvent(false))
            using (var server = NetSocketFactory.Default.CreateServer())
            {
                server.Listening += (s, e) => 
                {
                    signal.Set();
                };

                var run = server.ListenAsync(ServerPort);
                signal.WaitOne(2000);
                Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
                Assert.Equal(ServerPort.ToString(), server.LocalEndPoint.Port);
                server.Close();
            }
        }

        [Fact]
        public void TestClosed() 
        {
            
        }
    }
}
