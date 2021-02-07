using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Matches.Messages;
using Network.Messages;

namespace Matches.States
{
    public abstract class ListenSessionStateBase
    {
        protected ListenSessionMatch Context { get; }

        public ListenSessionStateBase(ListenSessionMatch context)
        {
            Context = context;
        }

        public abstract Task ProcessMessageAsync(IPEndPoint ip, byte[] received);

        protected bool IsHost(IPEndPoint ip)
            => Context.Host.IsClientPublicEndPoint(ip);

        protected bool IsClient(IPEndPoint ip)
            => Context.Clients.Any(o => o.IsClientPublicEndPoint(ip));
    }
}
