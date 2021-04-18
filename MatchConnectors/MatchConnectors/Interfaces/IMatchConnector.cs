using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Network.Messages;

namespace Connectors.MatchConnectors
{
    public interface IMatchConnector
    {
        Task<ConnectionMessage> ConnectAsync(UdpClient client, string serverIp, int serverPort, CancellationToken token = default);
    }
}
