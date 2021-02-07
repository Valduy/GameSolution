using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Network.Messages;

namespace Matches.Messages
{
    public class P2PConnectionMessage
    {
        public Role Role { get; set; }
        public List<ClientEndPoints> Clients { get; set; }
    }
}
