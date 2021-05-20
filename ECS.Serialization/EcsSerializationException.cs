using System;

namespace ECS.Serialization
{
    public class EcsSerializationException : Exception
    {
        public EcsSerializationException() {}

        public EcsSerializationException(string message) 
            : base(message)
        { }

        public EcsSerializationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
