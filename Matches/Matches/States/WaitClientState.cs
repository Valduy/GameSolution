using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Network;

namespace Matches.Matches.States
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
                if (!Context.Clients.Any(o => o.Equals(ip)))
                {
                    Context.AddClient(ip);

                    if (Context.Clients.Count >= Context.PlayersCount)
                    {
                        Context.State = new ChooseHostState(Context);
                        await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                    }
                }

                await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
            }
        }
    }
}
