using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network.Messages;

namespace Connectors.HolePuncher
{
    public interface IHolePuncher
    {
        Task<List<IPEndPoint>> ConnectAsync(
            UdpClient udpClient,
            uint sessionId,
            IEnumerable<ClientEndPoints> clients,
            CancellationToken cancellationToken = default);
    }
}
