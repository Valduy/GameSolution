﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Network.Messages;

namespace Matches
{
    public interface IMatch : IDisposable
    {
        int Port { get; }
        IEnumerable<ClientEndPoints> ExpectedPlayers { get; }

        event Action<IMatch> MatchStarted;
        
        Task WorkAsync(CancellationToken cancellationToken = default);
    }
}
