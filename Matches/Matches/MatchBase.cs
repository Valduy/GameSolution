using System;

namespace Matches.Matches
{
    public abstract class MatchBase
    {
        public int Port { get; protected set; }
        public bool IsWork { get; private set; }
        public int PlayersCount { get; }
        public long TimeForStarting { get; }

        public event Action<MatchBase> Started;
        public event Action<MatchBase> Ended;

        public MatchBase(int playersCount, long timeForStarting)
        {
            PlayersCount = playersCount;
            TimeForStarting = timeForStarting;
        }

        public MatchBase(int playersCount, long timeForStarting, int port) 
            : this(playersCount, timeForStarting) 
            => Port = port;

        public virtual void Start()
        {
            IsWork = true;
            Started?.Invoke(this);
        }

        public virtual void Stop()
        {
            IsWork = false;
            Ended?.Invoke(this);
        }
    }
}
