using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Network;

namespace Connectors.MatchmakerConnectors.Implementations.States
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
            // TODO: некорректный ответ
            var response = await Context.GetAsync("api/status");
            var data = await response.Content.ReadAsStringAsync();
            return (UserStatus)int.Parse(data);
        }
    }
}
