using System;
using System.Linq;

namespace Network
{
    public static class PacketHelper
    {
        public const int SessionIdSize = sizeof(uint);
        public const int NumberSize = sizeof(uint);
        public const int HeaderSize = SessionIdSize + NumberSize;

        public static byte[] CreatePacket(uint sessionId, uint number)
        {
            var packet = new byte[HeaderSize];
            BytesHelper.WriteUInt32(sessionId, packet);
            BytesHelper.WriteUInt32(number, packet, SessionIdSize);
            return packet;
        }

        public static byte[] CreatePacket(uint sessionId, uint number, byte[] data)
        {
            var packet = new byte[HeaderSize + data.Length];
            BytesHelper.WriteUInt32(sessionId, packet);
            BytesHelper.WriteUInt32(number, packet, sizeof(uint));
            BytesHelper.WriteData(data, packet, HeaderSize);
            return packet;
        }

        public static uint GetSessionId(byte[] packet) 
            => BytesHelper.ReadUInt32(packet, 0);

        public static uint GetNumber(byte[] packet) 
            => BytesHelper.ReadUInt32(packet, 0);

        public static byte[] GetData(byte[] packet) 
            => BytesHelper.ReadBytes(packet, HeaderSize, packet.Length - HeaderSize);

        public static bool IsShouldCorrectPacketNumber(uint[] numbers, double tolerance) 
            => Variance(numbers) < tolerance;

        private static double Variance(uint[] numbers)
        {
            var average = numbers.Average(n => n);
            return numbers.Sum(n => Math.Pow(n - average, 2)) / numbers.Length;
        }
    }
}
