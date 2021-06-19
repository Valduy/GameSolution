using System.Net;
using Network.NetworkBuffers;

namespace Network.Proxy
{
    public interface IClientNetworkProxy : INetworkProxy
    {
        IPEndPoint Host { get; }
        IWriteOnlyNetworkBuffer WriteBuffer { get; }
        IReadOnlyNetworkBuffer ReadBuffer { get; }
    }
}
