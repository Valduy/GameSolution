using System.Threading.Tasks;

namespace Connectors.MatchmakerConnectors
{
    internal abstract class MatchmakerConnectorStateBase
    {
        protected MatchmakerConnector Context { get; }

        public MatchmakerConnectorStateBase(MatchmakerConnector context) 
            => Context = context;

        public abstract Task ConnectAsync();
    }
}
