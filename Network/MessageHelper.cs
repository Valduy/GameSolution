using System;
using System.Text;

namespace Network
{
    public static class MessageHelper
    {
        public const int HeaderSize = sizeof(uint);

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
            BytesHelper.WriteUInt32((uint)type, message);
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
            BytesHelper.WriteUInt32((uint)type, message);
            BytesHelper.WriteData(data, message, HeaderSize);
            return message;
        }

        /// <summary>
        /// Метод создает сообщение с указанными данными.
        /// </summary>
        /// <param name="type"><see cref="NetworkMessages"/>.</param>
        /// <param name="data">Строка с данными (работает только с ASCII символами).</param>
        /// <returns>Сообщение.</returns>
        public static byte[] GetMessage(NetworkMessages type, string data)
        {
            var message = new byte[HeaderSize + data.Length];
            BytesHelper.WriteUInt32((uint)type, message);
            BytesHelper.WriteData(data, message, HeaderSize);
            return message;
        }

        /// <summary>
        /// Метод извлекает данные из сообщения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <returns>Данные.</returns>
        public static byte[] ToByteArray(byte[] message) =>
            BytesHelper.ReadBytes(message, HeaderSize, message.Length - HeaderSize);

        public static string ToString(byte[] message) 
            => Encoding.ASCII.GetString(message, HeaderSize, message.Length - HeaderSize);
    }
}
