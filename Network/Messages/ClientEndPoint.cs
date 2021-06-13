namespace Network.Messages
{
    public class ClientEndPoint
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        
        public ClientEndPoint()
        { }

        public ClientEndPoint(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }
    }
}
