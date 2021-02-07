using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Matches.Messages
{
    public class P2PConnectionMessage
    {
        public Role Role { get; set; }
        public List<ClientEndPoints> Clients { get; set; }
    }
}
