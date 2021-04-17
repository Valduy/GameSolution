namespace Network.Messages
{
    public class ClientEndPoints
    {
        public ClientEndPoint PublicEndPoint { get; set; }
        public ClientEndPoint PrivateEndPoint { get; set; }

        public ClientEndPoints() { }

        public ClientEndPoints(ClientEndPoint publicEndPoint, ClientEndPoint privateEndPoint)
        {
            PublicEndPoint = publicEndPoint;
            PrivateEndPoint = privateEndPoint;
        }

        public ClientEndPoints(string publicIp, int publicPort, string privateIp, int privatePort)
        {
            PublicEndPoint = new ClientEndPoint(publicIp, publicPort);
            PrivateEndPoint = new ClientEndPoint(privateIp, privatePort);
        }
    }
}
