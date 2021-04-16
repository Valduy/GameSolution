using System;
using Matches;

namespace Matchmaker.Factories.Implementations
{
    public class ListenMatchFactory : IMatchFactory
    {
        public IMatch CreateMatch(int playersCount)
            => new ListenSessionMatch(playersCount);
    }
}
