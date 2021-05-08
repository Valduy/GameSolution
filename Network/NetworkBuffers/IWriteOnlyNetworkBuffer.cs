namespace Network.NetworkBuffers
{
    public interface IWriteOnlyNetworkBuffer
    {
        bool IsEmpty { get; }
        bool IsFull { get; }
        int Size { get; }
        int Count { get; }

        void Write(byte[] message);
    }
}
