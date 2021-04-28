using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Connectors.MatchmakerConnectors
{
    internal class EnqueueState : MatchmakerConnectorStateBase
    {
        private readonly string _json;

        public EnqueueState(MatchmakerConnector context)
            : base(context) 
            => _json = JsonConvert.SerializeObject(Context.PrivateEndPoint);

        public override async Task ConnectAsync()
        {
            var response = await Context.PostAsync("api/queue", _json);

            if (response.IsSuccessStatusCode)
            {
                Context.State = new WaitState(Context);
            }
            else
            {
                throw new HttpConnectorException("Не удалось встать в очередь", response.StatusCode);
            }
        }
    }
}
