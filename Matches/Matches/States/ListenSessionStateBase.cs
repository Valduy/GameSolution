using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Matches.Messages;

namespace Matches.Matches.States
{
    public abstract class ListenSessionStateBase
    {
        protected ListenSessionMatch Context { get; }

        public ListenSessionStateBase(ListenSessionMatch context)
        {
            Context = context;
        }

        public abstract Task ProcessMessageAsync(IPEndPoint ip, byte[] received);

        protected static bool IsClientEndPoint(ClientEndPoints endPoints, IPEndPoint ip)
            => endPoints.PublicIp == ip.Address.ToString() && endPoints.PublicPort == ip.Port;

        protected bool IsHost(IPEndPoint ip)
            => IsClientEndPoint(Context.Host, ip);

        protected bool IsClient(IPEndPoint ip)
            => Context.Clients.Any(o => IsClientEndPoint(o, ip));
    }
}
