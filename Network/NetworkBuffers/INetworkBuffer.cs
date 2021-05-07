namespace Network.NetworkBuffers
{
    public interface INetworkBuffer
    {
        bool IsEmpty { get; }
        bool IsFull { get; }
        int Size { get; }
        int Count { get; }

        void Write(byte[] message);
        byte[] Read();
        byte[] ReadLast();
    }
}
