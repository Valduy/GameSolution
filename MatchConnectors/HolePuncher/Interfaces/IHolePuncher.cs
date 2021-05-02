using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network.Messages;

namespace Connectors.HolePuncher
{
    interface IHolePuncher
    {
        Task<List<IPEndPoint>> ConnectAsync(
            UdpClient udpClient,
            IEnumerable<ClientEndPoints> clients,
            CancellationToken cancellationToken = default);
    }
}
