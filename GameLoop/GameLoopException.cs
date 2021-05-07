using System;

namespace GameLoops
{
    public class GameLoopException : Exception
    {
        public GameLoopException() {}

        public GameLoopException(string message) 
            : base(message)
        { }
    }
}
