namespace ECS.Serialization.Writers
{
    public interface ISequentialWriter
    {
        void WriteString(string token);
        void WriteInt32(int num);
        void WriteFloat(float num);
    }
}
