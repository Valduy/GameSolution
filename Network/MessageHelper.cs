using System;
using System.Collections.Generic;
using System.Text;
//using System.Text.Json;

namespace Network
{
    public static class MessageHelper
    {
        public static readonly int HeaderSize = sizeof(int);

        public static NetworkMessages GetMessageType(byte[] message)
            => (NetworkMessages)BitConverter.ToUInt32(message, 0);

        /// <summary>
        /// Метод создает пустое сообщение указанного типа.
        /// </summary>
        /// <param name="type"><see cref="NetworkMessages"/>.</param>
        /// <returns>Пустое сообщение.</returns>
        public static byte[] GetMessage(NetworkMessages type)
        {
            var message = new byte[HeaderSize];
            WriteMessageType(type, message);
            return message;
        }

        /// <summary>
        /// Метод создает сообщение с указанными данными.
        /// </summary>
        /// <param name="type"><see cref="NetworkMessages"/>.</param>
        /// <param name="data">Данные.</param>
        /// <returns>Сообщение.</returns>
        public static byte[] GetMessage(NetworkMessages type, byte[] data)
        {
            var message = new byte[HeaderSize + data.Length];
            WriteMessageType(type, message);
            WriteData(data, message);
            return message;
        }

        /// <summary>
        /// Метод извлекает данные из сообщения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <returns>Данные.</returns>
        public static byte[] FromMessage(byte[] message)
        {
            var data = new byte[message.Length - HeaderSize];
            Array.Copy(message, HeaderSize, data, 0, data.Length);
            return data;
        }

        private static void WriteMessageType(NetworkMessages type, byte[] message)
        {
            uint value = (uint) type;

            for (int i = 0; i < HeaderSize; i++)
            {
                message[i] = (byte)(value & 0x000000ff);
                value >>= 8;
            }
        }

        private static void WriteData(byte[] data, byte[] message)
        {
            int startIndex = HeaderSize;

            for (int i = 0; i < data.Length; i++)
            {
                message[startIndex + i] = data[i];
            }
        }
    }
}
