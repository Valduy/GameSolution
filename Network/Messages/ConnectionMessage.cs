﻿using System.Collections.Generic;

namespace Network.Messages
{
    public class ConnectionMessage
    {
        public Role Role { get; set; }
        public List<ClientEndPoints> Clients { get; set; }
    }
}