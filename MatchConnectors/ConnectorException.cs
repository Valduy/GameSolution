using System;

namespace Connectors
{
    [Serializable]
    public class ConnectorException : Exception
    {
        public ConnectorException() { }

        public ConnectorException(string message) 
            : base(message) 
        { }

        public ConnectorException(string message, Exception inner) 
            : base(message, inner)
        { }
    }
}
