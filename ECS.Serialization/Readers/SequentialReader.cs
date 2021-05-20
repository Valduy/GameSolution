using System.Collections.Generic;
using System.Linq;

namespace ECS.Serialization.Readers
{
    public class SequentialReader : ISequentialReader
    {
        private readonly LinkedList<string> _sequence;

        public bool IsEmpty => _sequence.Any();

        public SequentialReader(LinkedList<string> sequence) => _sequence = sequence;

        public string Peek() => _sequence.First.Value;

        public string ReadString()
        {
            var first = _sequence.First.Value;
            _sequence.RemoveFirst();
            return first;
        }

        public int ReadInt32() => int.Parse(ReadString());

        public float ReadFloat() => float.Parse(ReadString());
    }
}
