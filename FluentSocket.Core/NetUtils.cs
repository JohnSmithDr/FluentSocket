using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FluentSocket
{
    static class NetUtils
    {
        public static NetEndPoint ToNetEndPoint(this EndPoint ep)
        {
            var ip = ep as IPEndPoint;
            return new NetEndPoint(ip?.Address.ToString(), ip?.Port.ToString());
        }

        public static async Task ConnectAsync(this Socket socket, string host, int port)
        {
            var tcs = new TaskCompletionSource<bool>();
            var args = new SocketAsyncEventArgs();
            try
            {
                args.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
                args.Completed += (s, e) => tcs.TrySetResult(true);

                var isAsync = socket.ConnectAsync(args);
                if (!isAsync) tcs.TrySetResult(false);
                await tcs.Task;
            }
            catch
            {
                throw;
            }
            finally
            {
                args.Dispose();
            }
        }

        public static Task<int> SendAsync(this Socket socket, byte[] buffer)
            => SendAsync(socket, buffer, 0, buffer.Length);

        public static async Task<int> SendAsync(this Socket socket, byte[] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<int>();
            var args = new SocketAsyncEventArgs();
            try
            {
                args.SetBuffer(buffer, offset, count);
                args.Completed += (s, e) => tcs.TrySetResult(e.BytesTransferred);

                var isAsync = socket.SendAsync(args);
                if (!isAsync) tcs.TrySetResult(args.BytesTransferred);

                return await tcs.Task;
            }
            catch
            {
                throw;
            }
            finally
            {
                args.TryDispose();
            }
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<int>();
            var args = new SocketAsyncEventArgs();
            try
            {
                args.SetBuffer(buffer, offset, count);
                args.Completed += (s, e) => tcs.TrySetResult(e.BytesTransferred);

                var isAsync = socket.ReceiveAsync(args);
                if (!isAsync) tcs.TrySetResult(args.BytesTransferred);

                return await tcs.Task;
            }
            catch
            {
                throw;
            }
            finally
            {
                args.TryDispose();
            }
        }
    }
}