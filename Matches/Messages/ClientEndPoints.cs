using System;
using System.Collections.Generic;
using System.Text;

namespace Matches.Messages
{
    public class ClientEndPoints : ClientPrivateEndPoint
    {
        public string PublicIp { get; }
        public int PublicPort { get; }

        public ClientEndPoints(string publicIp, int publicPort, string privateIp, int privatePort) 
            : base(privateIp, privatePort)
        {
            PublicIp = publicIp;
            PublicPort = publicPort;
        }

        public override bool Equals(object obj) 
            => Equals(obj as ClientEndPoints);
        
        public bool Equals(ClientEndPoints other) 
            => base.Equals(other) 
               && PublicIp == other.PublicIp 
               && PublicPort == other.PublicPort;

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (PublicIp != null ? PublicIp.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PublicPort;
                return hashCode;
            }
        }
    }
}
