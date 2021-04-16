using System;
using System.Threading;
using System.Threading.Tasks;

namespace Matches
{
    public interface IMatch
    {
        int Port { get; }
        int PlayersCount { get; }

        event Action<IMatch> MatchStarted;
        
        Task WorkAsync(CancellationToken token = default);
    }
}
