using System;
using System.Collections.Generic;
using System.Text;

namespace Matches
{
    public interface IMatch
    {
        int Port { get; }
        long TimeForStarting { get; set; }

        event Action Started;
        event Action Ended;
    }
}
