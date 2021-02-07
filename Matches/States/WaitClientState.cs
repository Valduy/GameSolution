using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Matches.Messages;
using Network;
using Network.Messages;

namespace Matches.States
{
    public class WaitClientState : ListenSessionStateBase
    {
        public WaitClientState(ListenSessionMatch context) : base(context)
        {
        }

        public override async Task ProcessMessageAsync(IPEndPoint ip, byte[] received)
        {
            if (MessageHelper.GetMessageType(received) == NetworkMessages.Hello)
            {
                if (!IsClient(ip))
                {
                    Context.AddClient(CreateClientEndPoints(ip, received));
                    
                    if (Context.Clients.Count >= Context.PlayersCount)
                    {
                        Context.State = new ChooseHostState(Context);
                        await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                    }
                }

                await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
            }
        }

        private ClientEndPoints CreateClientEndPoints(IPEndPoint ip, byte[] received)
        {
            var data = MessageHelper.ToString(received);
            // TODO: что если предадут что-то не то...
            var privateEndPoint = JsonSerializer.Deserialize<ClientEndPoint>(data);

            return new ClientEndPoints(
                ip.Address.ToString(),
                ip.Port,
                privateEndPoint.Ip,
                privateEndPoint.Port
            );
        }
    }
}
