using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Network;
using Network.Messages;

namespace Matches.States
{
    public class ConnectClientsState : ListenSessionStateBase
    {
        private readonly byte[] _hostMessage;
        private readonly byte[] _clientMessage;

        public ConnectClientsState(ListenSessionMatch context) : base(context)
        {
            var messageForHost = new ConnectionMessage
            {
                SessionId = Context.SessionId,
                Role = Role.Host,
                Clients = Context.Clients.ToList(),
            };
            _hostMessage = MessageHelper.GetMessage(NetworkMessages.Initial, JsonSerializer.Serialize(messageForHost));

            var messageForClient = new ConnectionMessage
            {
                SessionId = Context.SessionId,
                Role = Role.Client,
                Clients = new List<ClientEndPoints> {Context.Host}
            };
            _clientMessage = MessageHelper.GetMessage(NetworkMessages.Initial, JsonSerializer.Serialize(messageForClient));
        }

        public override async Task ProcessMessageAsync(IPEndPoint ip, byte[] received)
        {
            if (MessageHelper.GetMessageType(received) == NetworkMessages.Hello)
            {
                Context.LogInformation("Соединение...");

                if (IsHost(ip))
                {
                    await Context.SendMessageAsync(_hostMessage, ip);
                }
                else if (IsClient(ip))
                {
                    await Context.SendMessageAsync(_clientMessage, ip);
                }
            }
            else
            {
                Context.LogInformation("Пришло неизвестное сообщение.");
            }
        }
    }
}
