using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Connectors.MatchConnectors.States
{
    public abstract class MatchConnectorStateBase
    {
        protected MatchConnector Context { get; }

        public MatchConnectorStateBase(MatchConnector context) 
            => Context = context;

        public abstract Task SendMessageAsync();
        public abstract void ProcessMessage(byte[] message);
    }
}
