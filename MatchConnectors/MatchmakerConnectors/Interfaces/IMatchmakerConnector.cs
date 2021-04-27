using System.Threading;
using System.Threading.Tasks;
using Network.Messages;

namespace Connectors.MatchmakerConnectors
{
    public interface IMatchmakerConnector
    {
        Task<int?> ConnectAsync(
            ClientEndPoint privateEndPoint,
            string host,
            string bearerToken,
            CancellationToken cancellationToken = default);
    }
}
