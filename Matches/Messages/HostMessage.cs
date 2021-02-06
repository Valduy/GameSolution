using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Matches.Messages
{
    public class HostMessage
    {
        public List<ClientMessage> Clients { get; set; }
    }
}
