namespace Network
{
    public interface IReadOnlyNetworkBuffer
    {
        bool IsEmpty { get; }
        bool IsFull { get; }
        int Size { get; }
        int Count { get; }

        byte[] Read();
        byte[] ReadLast();
    }
}
