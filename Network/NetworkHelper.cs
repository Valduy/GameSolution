using System;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    public static class NetworkHelper
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static int GetPort(this UdpClient udpClient) 
            => ((IPEndPoint) udpClient.Client.LocalEndPoint).Port;
    }
}
