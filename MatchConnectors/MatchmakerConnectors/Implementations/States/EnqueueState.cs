using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Connectors.MatchmakerConnectors.Implementations.States
{
    internal class EnqueueState : MatchmakerConnectorStateBase
    {
        private readonly string _json;

        public EnqueueState(MatchmakerConnector context)
            : base(context) 
            => _json = JsonConvert.SerializeObject(Context.PrivateEndPoint);

        public override async Task ConnectAsync()
        {
            await Context.PostAsync("api/queue", _json);
            Context.State = new WaitState(Context);
        }
    }
}
