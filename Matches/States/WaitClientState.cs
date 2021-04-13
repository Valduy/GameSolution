using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
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
                    try
                    {
                        Context.AddClient(CreateClientEndPoints(ip, received));

                        if (Context.Clients.Count >= Context.PlayersCount)
                        {
                            Context.State = new ChooseHostState(Context);
                            Context.NotifyThatStarted();
                            await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is JsonException || ex is ArgumentException)
                        {
                            return; // TODO: лог о неправильном JSON'е?
                        }
                    }
                }

                await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
            }
        }

        private ClientEndPoints CreateClientEndPoints(IPEndPoint ip, byte[] received)
        {
            var data = MessageHelper.ToString(received);
            var privateEndPoint = JsonSerializer.Deserialize<ClientEndPoint>(data) 
                                  ?? throw new ArgumentException("При десериализации конечной точки был получен null.");

            return new ClientEndPoints(
                ip.Address.ToString(),
                ip.Port,
                privateEndPoint.Ip,
                privateEndPoint.Port
            );
        }
    }
}
