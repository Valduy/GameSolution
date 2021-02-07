using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Connectors.MatchConnectors.States
{
    public abstract class P2PMatchConnectorStateBase
    {
        protected P2PMatchConnector Context { get; }

        public P2PMatchConnectorStateBase(P2PMatchConnector context) 
            => Context = context;

        public abstract void ProcessMessage(IPEndPoint ip, byte[] message);

        public abstract Task SendMessageAsync();
    }
}
