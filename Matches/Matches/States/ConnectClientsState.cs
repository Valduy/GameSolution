using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Matches.Messages;
using Network;

namespace Matches.Matches.States
{
    public class ConnectClientsState : ListenSessionStateBase
    {
        private readonly byte[] _hostMessage;
        private readonly byte[] _clientMessage;

        public ConnectClientsState(ListenSessionMatch context) : base(context)
        {
            var messageForHost = new HostMessage
            {
                Clients = Context.Clients
                    .Select(o => new ClientMessage {Ip = o.Address.ToString(), Port = o.Port})
                    .ToList()
            };
            _hostMessage = MessageHelper.GetMessage(NetworkMessages.Host, JsonSerializer.Serialize(messageForHost));

            var messageForClient = new ClientMessage
            {
                Ip = Context.Host.Address.ToString(), 
                Port = Context.Host.Port
            };
            _clientMessage = MessageHelper.GetMessage(NetworkMessages.Client, JsonSerializer.Serialize(messageForClient));
        }

        public override async Task ProcessMessageAsync(IPEndPoint ip, byte[] received)
        {
            if (MessageHelper.GetMessageType(received) == NetworkMessages.Hello)
            {
                if (Context.Host.Equals(ip))
                {
                    await Context.SendMessageAsync(_hostMessage, ip);
                }
                else if (Context.Clients.Any(o => o.Equals(ip)))
                {
                    await Context.SendMessageAsync(_clientMessage, ip);
                }
            }
        }
    }
}
