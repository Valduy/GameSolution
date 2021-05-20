namespace ECS.Serialization.Writers
{
    public interface ISequentialWriter
    {
        void WriteBool(bool value);
        void WriteByte(byte value);
        void WriteInt16(short value);
        void WriteInt32(int value);
        void WriteInt64(long value);
        void WriteUInt16(ushort value);
        void WriteUInt32(uint value);
        void WriteUInt64(ulong value);
        void WriteFloat(float value);
        void WriteDouble(double value);
        void WriteString(string token);
    }
}
