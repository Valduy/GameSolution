using System;
using System.Collections.Generic;
using System.Text;

namespace Matches.Messages
{
    public class ClientPrivateEndPoint
    {
        public string PrivateIp { get; }
        public int PrivatePort { get; }

        public ClientPrivateEndPoint(string privateIp, int privatePort)
        {
            PrivateIp = privateIp;
            PrivatePort = privatePort;
        }

        public override bool Equals(object obj) 
            => Equals(obj as ClientPrivateEndPoint);

        public bool Equals(ClientPrivateEndPoint other)
            => other != null
               && PrivateIp == other.PrivateIp
               && PrivatePort == other.PrivatePort;

        public override int GetHashCode()
        {
            unchecked
            {
                return ((PrivateIp != null ? PrivateIp.GetHashCode() : 0) * 397) ^ PrivatePort;
            }
        }
    }
}
