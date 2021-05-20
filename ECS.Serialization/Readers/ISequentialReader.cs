namespace ECS.Serialization.Readers
{
    public interface ISequentialReader
    {
        bool IsEmpty { get; }

        string Peek();
        string ReadString();
        int ReadInt32();
        float ReadFloat();
    }
}
