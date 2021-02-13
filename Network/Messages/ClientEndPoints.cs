namespace Network.Messages
{
    public class ClientEndPoints
    {
        public ClientEndPoint PublicEndPoint { get; }
        public ClientEndPoint PrivateEndPoint { get; }

        public ClientEndPoints(string publicIp, int publicPort, string privateIp, int privatePort)
        {
            PublicEndPoint = new ClientEndPoint(publicIp, publicPort);
            PrivateEndPoint = new ClientEndPoint(privateIp, privatePort);
        }

        public override bool Equals(object obj) 
            => Equals(obj as ClientEndPoints);

        public bool Equals(ClientEndPoints other) 
            => other != null 
               && PublicEndPoint.Equals(other.PublicEndPoint) 
               && PrivateEndPoint.Equals(other.PrivateEndPoint);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((PublicEndPoint != null ? PublicEndPoint.GetHashCode() : 0) * 397) ^ (PrivateEndPoint != null ? PrivateEndPoint.GetHashCode() : 0);
            }
        }
    }
}
