using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ECS.Serialization.Readers
{
    public class SequentialReader : ISequentialReader
    {
        private readonly LinkedList<string> _sequence;

        public bool IsEmpty => _sequence.Any();

        public SequentialReader(LinkedList<string> sequence) => _sequence = sequence;

        public string Peek() => _sequence.First.Value;

        public bool ReadBool() => bool.Parse(ReadString());

        public byte ReadByte() => byte.Parse(ReadString());

        public short ReadInt16() => short.Parse(ReadString());

        public int ReadInt32() => int.Parse(ReadString());

        public long ReadInt64() => long.Parse(ReadString());

        public ushort ReadUInt16() => ushort.Parse(ReadString());

        public uint ReadUInt32() => uint.Parse(ReadString());

        public ulong ReadUInt64() => ulong.Parse(ReadString());

        public float ReadFloat() => float.Parse(ReadString(), CultureInfo.InvariantCulture);

        public double ReadDouble() => double.Parse(ReadString(), CultureInfo.InvariantCulture);

        public string ReadString()
        {
            var first = _sequence.First.Value;
            _sequence.RemoveFirst();
            return first;
        }
    }
}
