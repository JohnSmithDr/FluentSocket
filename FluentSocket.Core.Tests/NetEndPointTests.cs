using FluentSocket;
using Xunit;

namespace Tests
{
    public class NetEndPointTests
    {
        [Fact]
        public void TestToString()
        {
            var ep = new NetEndPoint("127.0.0.1", "80");
            Assert.Equal("127.0.0.1:80", ep.ToString());
        }
    }
}