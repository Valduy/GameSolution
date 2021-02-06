using System.Net;
using System.Threading.Tasks;

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
    }
}
