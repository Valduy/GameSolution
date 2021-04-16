using System.Net;

namespace Network.Messages
{
    public static class ClientEndPointsHelper
    {
        public static bool IsSame(this ClientEndPoint thisEndPoint, IPEndPoint otherEndPoint)
            => thisEndPoint.Ip == otherEndPoint.Address.ToString()
               && thisEndPoint.Port == otherEndPoint.Port;

        public static bool IsSame(this ClientEndPoint thisEndPoint, ClientEndPoint otherEndPoint)
            => thisEndPoint.Ip == otherEndPoint.Ip
               && thisEndPoint.Port == otherEndPoint.Port;

        public static bool IsSame(this ClientEndPoints thisEndPoints, ClientEndPoints otherEndPoints)
            => thisEndPoints.PublicEndPoint.IsSame(otherEndPoints.PublicEndPoint)
               && thisEndPoints.PrivateEndPoint.IsSame(otherEndPoints.PrivateEndPoint);

        public static bool IsClientPublicEndPoint(this ClientEndPoints endPoints, IPEndPoint otherEndPoint)
            => endPoints.PublicEndPoint.IsSame(otherEndPoint);

        public static bool IsClientPrivateEndPoint(this ClientEndPoints endPoints, IPEndPoint otherEndPoint)
            => endPoints.PrivateEndPoint.IsSame(otherEndPoint);
    }
}
