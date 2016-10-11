using Xunit;
using FluentSocket;
using System.Threading.Tasks;

namespace Tests
{
    public class NetSocketServerTests
    {
        const int ServerPort = 3000;

        [Fact]
        public async Task TestListenAsync()
        {
            var server = NetSocketFactory.Default.CreateServer();
            var run = server.ListenAsync(ServerPort);
            await Task.Delay(50);
            //Assert.Equal("0.0.0.0", server.LocalEndPoint.Host);
            //Assert.Equal(ServerPort.ToString(), server.LocalEndPoint.Port);
            server.Close();
            server.Dispose();
        }

        [Fact]
        public void TestClosed() 
        {
            
        }
    }
}
