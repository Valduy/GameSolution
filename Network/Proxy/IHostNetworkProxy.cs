using System.Collections.Generic;
using System.Net;
using Network.NetworkBuffers;

namespace Network.Proxy
{
    public interface IHostNetworkProxy : INetworkProxy
    {
        IEnumerable<IPEndPoint> Clients { get; }
        IReadOnlyNetworkBuffer GetReadBuffer(IPEndPoint endPoint);
        IWriteOnlyNetworkBuffer GetWriteBuffer(IPEndPoint endPoint);
    }
}
