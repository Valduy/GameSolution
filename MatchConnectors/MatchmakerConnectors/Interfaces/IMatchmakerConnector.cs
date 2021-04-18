using System.Threading;
using System.Threading.Tasks;

namespace Connectors.MatchmakerConnectors.Interfaces
{
    interface IMatchmakerConnector
    {
        Task<int> ConnectAsync(int port, string authToken, CancellationToken token = default);
    }
}
