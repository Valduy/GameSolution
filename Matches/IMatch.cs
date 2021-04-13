using System;
using System.Threading;
using System.Threading.Tasks;

namespace Matches
{
    public interface IMatch
    {
        int Port { get; }
        int PlayersCount { get; }
        long TimeForStarting { get; }

        event Action<IMatch> MatchStarted;

        Task WorkAsync(int playersCount, CancellationToken token = default);
        Task WorkAsync(int playersCount, long timeForStarting, CancellationToken token = default);
        Task WorkAsync(int playersCount, long timeForStarting, int port, CancellationToken token = default);
    }
}
