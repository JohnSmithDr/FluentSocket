using System.Threading.Tasks;

namespace FluentSocket
{
    public interface ISocketFactory
    {
        Task<ISocket> ConnectAsync(string host, int port);
        ISocketServer CreateServer();
    }
}