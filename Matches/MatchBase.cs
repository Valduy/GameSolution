using System;
using System.Threading;
using System.Threading.Tasks;

namespace Matches
{
    public abstract class MatchBase
    {
        public int Port { get; protected set; }
        public int PlayersCount { get; }
        public long TimeForStarting { get; }
        
        public MatchBase(int playersCount, long timeForStarting)
        {
            PlayersCount = playersCount;
            TimeForStarting = timeForStarting;
        }

        public MatchBase(int playersCount, long timeForStarting, int port) 
            : this(playersCount, timeForStarting) 
            => Port = port;

        public abstract Task WorkAsync(CancellationToken token = default);
    }
}
