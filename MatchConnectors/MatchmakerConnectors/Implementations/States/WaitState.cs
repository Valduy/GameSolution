using System.Threading.Tasks;
using Network;

namespace Connectors.MatchmakerConnectors
{
    internal class WaitState : MatchmakerConnectorStateBase
    {
        public WaitState(MatchmakerConnector context) 
            : base(context)
        { }

        public override async Task ConnectAsync()
        {
            switch (await GetUserStatus())
            {
                case UserStatus.Connected:
                    Context.State = new GetMatchState(Context);
                    break;
                case UserStatus.Absent:
                    Context.State = new EnqueueState(Context);
                    break;
            }
        }

        private async Task<UserStatus> GetUserStatus()
        {
            var response = await Context.GetAsync($"{Context.Host}/api/matchmaker/status");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpConnectorException("Не удалось получить статус пользователя", response.StatusCode);
            }

            var data = await response.Content.ReadAsStringAsync();
            return (UserStatus)int.Parse(data);
        }
    }
}
