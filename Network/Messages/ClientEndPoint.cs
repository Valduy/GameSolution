namespace Network.Messages
{
    public class ClientEndPoint
    {
        public string Ip { get; }
        public int Port { get; }

        public ClientEndPoint(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public override bool Equals(object obj)
            => Equals(obj as ClientEndPoint);

        public bool Equals(ClientEndPoint other)
            => other != null
               && Ip == other.Ip
               && Port == other.Port;

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Ip != null ? Ip.GetHashCode() : 0) * 397) ^ Port;
            }
        }
    }
}
