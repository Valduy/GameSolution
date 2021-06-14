using System;

namespace Network
{
    public static class PacketHelper
    {
        public const int HeaderSize = sizeof(uint);

        public static byte[] CreatePacket(uint number, byte[] data)
        {
            var packet = new byte[HeaderSize + data.Length];
            BytesHelper.WriteUInt32(number, packet);
            BytesHelper.WriteData(data, packet, HeaderSize);
            return packet;
        }

        public static uint GetNumber(byte[] packet) 
            => BitConverter.ToUInt32(packet, 0);

        public static byte[] GetData(byte[] packet) 
            => BytesHelper.ReadBytes(packet, HeaderSize, packet.Length - HeaderSize);
    }
}
