using System;
using System.Collections.Generic;
using System.Text;

namespace Matches
{
    public abstract class MatchBase
    {
        public int Port { get; protected set; }
        public bool IsWork { get; private set; }
        public long TimeForStarting { get; }

        public event Action<MatchBase> Started;
        public event Action<MatchBase> Ended;

        public MatchBase(long timeForStarting) 
            => TimeForStarting = timeForStarting;

        public MatchBase(long timeForStarting, int port) 
            : this(timeForStarting) 
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
