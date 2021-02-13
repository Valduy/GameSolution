using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matchmaker.Exceptions
{
    public class AddItemException : Exception
    {
        public AddItemException() {}

        public AddItemException(string message) 
            : base(message) {}
    }
}
