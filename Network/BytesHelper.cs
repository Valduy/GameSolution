using System;

namespace Network
{
    public static class BytesHelper
    {
        public static void WriteUInt32(uint value, byte[] destination, int offset = 0)
        {
            for (int i = 0; i < sizeof(uint); i++)
            {
                destination[offset + i] = (byte)(value & 0x000000ff);
                value >>= 8;
            }
        }

        public static void WriteData(byte[] source, byte[] destination, int offset = 0)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[offset + i] = source[i];
            }
        }

        public static void WriteData(string source, byte[] destination, int offset = 0)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[offset + i] = (byte)source[i];
            }
        }

        public static byte[] ReadBytes(byte[] source, int offset, int count)
        {
            var destination = new byte[count];
            Array.Copy(source, offset, destination, 0, count);
            return destination;
        } 
    }
}
