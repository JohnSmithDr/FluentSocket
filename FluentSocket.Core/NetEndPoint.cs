namespace FluentSocket
{
    public class NetEndPoint
    {
        public string Host { get; }
        public string Port { get; }

        public NetEndPoint (string host, string port)
        {
            Host = host;
            Port = port;
        }

        public override string ToString() => $"{Host}:{Port}";
    }
}