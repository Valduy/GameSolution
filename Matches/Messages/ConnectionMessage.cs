using System;
using System.Collections.Generic;
using System.Text;

namespace Matches.Messages
{
    

    public class ConnectionMessage
    {
        public Role Role { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
    }
}
