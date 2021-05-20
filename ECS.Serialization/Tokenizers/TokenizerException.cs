using System;

namespace ECS.Serialization.Tokenizers
{
    public class TokenizerException : EcsSerializationException
    {
        public TokenizerException() { }

        public TokenizerException(string message)
            : base(message)
        { }

        public TokenizerException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
