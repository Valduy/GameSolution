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
            var response = await Context.PutAsync($"{Context.Host}/api/matchmaker/queue", _json);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();

                if (bool.Parse(data))
                {
                    Context.State = new WaitState(Context);
                }
                else
                {
                    throw new ConnectorException("Пользователь уже нахоидся в очереди.");
                }
            }
            else
            {
                throw new HttpConnectorException("Не удалось встать в очередь", response.StatusCode);
            }
        }
    }
}
