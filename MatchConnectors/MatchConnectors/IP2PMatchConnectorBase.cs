using System.Threading;
using System.Threading.Tasks;

namespace Connectors.MatchConnectors
{
    public interface IP2PMatchConnectorBase
    {
        Task<P2PConnectionResult> ConnectAsync(string serverIp, int serverPort, CancellationToken token = default);
    }
}
