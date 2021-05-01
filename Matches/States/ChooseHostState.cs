using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Network;
using Network.Messages;

namespace Matches.States
{
    public class ChooseHostState : ListenSessionStateBase
    {
        public ChooseHostState(ListenSessionMatch context) : base(context)
        {
        }

        public override async Task ProcessMessageAsync(IPEndPoint ip, byte[] received)
        {
            if (MessageHelper.GetMessageType(received) == NetworkMessages.Hello)
            {
                Context.LogInformation("Выбор хоста...");

                if (TryGetClient(ip, out var client))
                {
                    Context.ChooseHost(client);
                    Context.LogInformation($"Пользователь {ip} назначен хостом.");
                    Context.State = new ConnectClientsState(Context);
                    Context.LogInformation("Начинается соединение...");
                    await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                }
                else
                {
                    Context.LogInformation($"Неожиданный ip:{ip}.");
                }
            }
            else
            {
                Context.LogInformation("Пришло неизвестное сообщение.");
            }
        }

        private bool TryGetClient(IPEndPoint ip, out ClientEndPoints endPoints)
        {
            endPoints = Context.Clients.FirstOrDefault(o => o.IsClientPublicEndPoint(ip));
            return endPoints != null;
        }
    }
}
