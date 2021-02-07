using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Matches.Messages;
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
                if (TryGetClient(ip, out var client))
                {
                    Context.ChooseHost(client);
                    Context.State = new ConnectClientsState(Context);
                    await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                }
            }
        }

        private bool TryGetClient(IPEndPoint ip, out ClientEndPoints endPoints)
        {
            endPoints = Context.Clients.FirstOrDefault(o => o.IsClientPublicEndPoint(ip));
            return endPoints != null;
        }
    }
}
