using System.Collections.Generic;
using Matches;
using Microsoft.Extensions.Logging;
using Network.Messages;

namespace Matchmaker.Factories.Implementations
{
    public class ListenMatchFactory : IMatchFactory
    {
        private readonly ILogger<ListenSessionMatch> _logger;

        public ListenMatchFactory(ILogger<ListenSessionMatch> logger) 
            => _logger = logger;

        public IMatch CreateMatch(IEnumerable<ClientEndPoints> playersEndPoints)
            => new ListenSessionMatch(playersEndPoints, _logger);
    }
}
