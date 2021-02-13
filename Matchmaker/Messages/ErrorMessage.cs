using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matchmaker.Messages
{
    public class ErrorMessage
    {
        public string Message { get; }

        public ErrorMessage(string message)
        {
            Message = message;
        }
    }
}
