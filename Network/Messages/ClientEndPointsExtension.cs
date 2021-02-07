using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Matches.Messages;

namespace Network.Messages
{
    public static class ClientEndPointsExtension
    {
        public static bool IsClientPublicEndPoint(this ClientEndPoints endPoints, IPEndPoint ip) 
            => endPoints.PublicEndPoint.Ip == ip.Address.ToString() 
               && endPoints.PublicEndPoint.Port == ip.Port;

        public static bool IsClientPrivateEndPoint(this ClientEndPoints endPoints, IPEndPoint ip)
            => endPoints.PrivateEndPoint.Ip == ip.Address.ToString()
               && endPoints.PrivateEndPoint.Port == ip.Port;
    }
}
