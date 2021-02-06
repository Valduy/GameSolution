using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Network;

namespace Matches.Matches.States
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
                var client = Context.Clients.FirstOrDefault(o => o.Equals(ip));

                if (client != null)
                {
                    Context.ChooseHost(client);
                    Context.State = new ConnectClientsState(Context);
                    await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                }
            }
        }
    }
}
