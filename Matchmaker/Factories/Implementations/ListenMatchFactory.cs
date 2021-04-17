using System;
using System.Collections.Generic;
using Matches;
using Network.Messages;

namespace Matchmaker.Factories.Implementations
{
    public class ListenMatchFactory : IMatchFactory
    {
        public IMatch CreateMatch(IEnumerable<ClientEndPoints> playersEndPoints)
            => new ListenSessionMatch(playersEndPoints);
    }
}
