using System;

namespace Matchmaker.Exceptions
{
    public class AddItemException : Exception
    {
        public AddItemException() {}

        public AddItemException(string message) 
            : base(message) {}
    }
}
