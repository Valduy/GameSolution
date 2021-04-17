using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Network.Messages;

namespace Matches
{
    public interface IMatch
    {
        int Port { get; }
        IEnumerable<ClientEndPoints> ExpectedPlayers { get; }

        event Action<IMatch> MatchStarted;
        
        Task WorkAsync(CancellationToken token = default);
    }
}
