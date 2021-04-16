using Matches;

namespace Matchmaker.Factories
{
    public interface IMatchFactory
    {
        IMatch CreateMatch(int playersCount);
    }
}
