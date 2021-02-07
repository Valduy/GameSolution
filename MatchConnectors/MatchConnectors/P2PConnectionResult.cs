using System.Collections.Generic;
using System.Net.Sockets;
using Matches.Messages;
using Network.Messages;

namespace Connectors.MatchConnectors
{
    public class P2PConnectionResult
    {
        public Role Role { get; }
        public UdpClient UdpClient { get; }
        public List<ClientEndPoint> Clients { get; }

        public P2PConnectionResult(Role role, UdpClient udpClient, List<ClientEndPoint> clients)
        {
            Role = role;
            UdpClient = udpClient;
            Clients = clients;
        }
    }
}
