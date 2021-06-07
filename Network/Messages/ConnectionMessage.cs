using System.Collections.Generic;

namespace Network.Messages
{
    public class ConnectionMessage
    {
        public uint SessionId { get; set; }
        public Role Role { get; set; }
        public List<ClientEndPoints> Clients { get; set; }
    }
}
