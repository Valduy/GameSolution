using System;
using System.Net;

namespace Matchmaker.Exceptions
{
    public class HttpStatusException : Exception
    {
        public HttpStatusCode Status { get; }

        public HttpStatusException(HttpStatusCode status, string message) 
            : base(message)
        {
            Status = status;
        }
    }
}
