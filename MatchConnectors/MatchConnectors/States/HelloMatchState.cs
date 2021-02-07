using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Matches.Messages;
using Network;
using Network.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Connectors.MatchConnectors.States
{
    public class HelloMatchState : P2PMatchConnectorStateBase
    {
        private readonly byte[] _message;

        public HelloMatchState(P2PMatchConnector context) : base(context)
        {
            var privateEndPoint = new ClientEndPoint(Context.Ip, Context.Port);
            var data = JsonConvert.SerializeObject(privateEndPoint);
            _message = MessageHelper.GetMessage(NetworkMessages.Hello, data);
        }

        public override void ProcessMessage(IPEndPoint ip, byte[] message)
        {
            // TODO: сервер долго не отвечает
            switch (MessageHelper.GetMessageType(message))
            {
                case NetworkMessages.Hello:
                    // TODO: сервер дал знать, что он все еще жив...
                    break;
                case NetworkMessages.Initial:
                    var data = MessageHelper.ToString(message);
                    var connectionMessage = JsonConvert.DeserializeObject<P2PConnectionMessage>(data);
                    Context.Role = connectionMessage.Role;
                    Context.PotentialEndPoints = connectionMessage.Clients;
                    Context.State = new HolePunchingState(Context);
                    break;
            }
        }

        public override async Task SendMessageAsync()
        {
            await Context.SendMessageAsync(_message, Context.ServerIp, Context.ServerPort);
        }
    }
}
