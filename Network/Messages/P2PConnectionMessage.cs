using System.Collections.Generic;

namespace Network.Messages
{
    public class P2PConnectionMessage
    {
        public Role Role { get; set; }
        public List<ClientEndPoints> Clients { get; set; }
    }
}
