using System;
using System.Net;

namespace Connectors
{
    public class HttpConnectorException : ConnectorException
    {
        public HttpStatusCode? StatusCode { get; }

        public HttpConnectorException(HttpStatusCode? statusCode = null) 
            => StatusCode = statusCode;

        public HttpConnectorException(string message, HttpStatusCode? statusCode = null)
            : base(message) 
            => StatusCode = statusCode;

        public HttpConnectorException(string message, Exception inner, HttpStatusCode? statusCode = null)
            : base(message, inner) 
            => StatusCode = statusCode;
    }
}
