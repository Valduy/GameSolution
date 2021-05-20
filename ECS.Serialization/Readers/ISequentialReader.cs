namespace ECS.Serialization.Readers
{
    public interface ISequentialReader
    {
        bool IsEmpty { get; }

        string Peek();
        bool ReadBool();
        byte ReadByte();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        float ReadFloat();
        double ReadDouble();
        string ReadString();
    }
}
