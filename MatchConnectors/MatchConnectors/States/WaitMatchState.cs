using System;
using System.Threading.Tasks;
using Network;
using Network.Messages;
using Newtonsoft.Json;

namespace Connectors.MatchConnectors.States
{
    public class WaitMatchState : MatchConnectorStateBase
    {
        private readonly byte[] _message;

        public WaitMatchState(MatchConnector context) 
            : base(context) 
            => _message = MessageHelper.GetMessage(NetworkMessages.Hello);

        public override async Task ProcessMessageAsync(byte[] message)
        {
            if (MessageHelper.GetMessageType(message) == NetworkMessages.Initial)
            {
                // TODO: JsonExceptions
                var data = MessageHelper.ToString(message);
                Context.ConnectionMessage = JsonConvert.DeserializeObject<ConnectionMessage>(data);
                Context.FinishConnection();
            }

            await Context.SendMessageAsync(_message);
        }
    }
}
