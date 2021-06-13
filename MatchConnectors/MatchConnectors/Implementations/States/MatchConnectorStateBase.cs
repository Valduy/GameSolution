using System.Threading.Tasks;

namespace Connectors.MatchConnectors
{
    internal abstract class MatchConnectorStateBase
    {
        protected MatchConnector Context { get; }

        public MatchConnectorStateBase(MatchConnector context) 
            => Context = context;

        public abstract Task SendMessageAsync();
        public abstract void ProcessMessage(byte[] message);
    }
}
