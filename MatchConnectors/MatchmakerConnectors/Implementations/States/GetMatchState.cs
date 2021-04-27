﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Connectors.MatchmakerConnectors.Implementations.States
{
    internal class GetMatchState : MatchmakerConnectorStateBase
    {
        public GetMatchState(MatchmakerConnector context) 
            : base(context)
        { }

        public override async Task ConnectAsync()
        {
            Context.MatchPort = await GetMatchPort();
            
            if (Context.MatchPort == null)
            {
                Context.State = new WaitState(Context);
            }
            else
            {
                Context.FinishConnection();
            }
        }

        private async Task<int?> GetMatchPort()
        {
            // TODO: некорректный ответ
            var response = await Context.GetAsync("api/match");
            var data = await response.Content.ReadAsStringAsync();
            return int.TryParse(data, out var number) ? (int?)number : null;
        }
    }
}
