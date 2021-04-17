using System.Collections.Generic;
using Matches;
using Network.Messages;

namespace Matchmaker.Factories
{
    public interface IMatchFactory
    {
        IMatch CreateMatch(IEnumerable<ClientEndPoints> playersEndPoints);
    }
}
