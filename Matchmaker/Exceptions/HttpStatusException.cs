using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
