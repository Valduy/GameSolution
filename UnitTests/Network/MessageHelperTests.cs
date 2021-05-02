using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Xunit;

namespace UnitTests.Network
{
    public class MessageHelperTests
    {
        [Theory]
        [ClassData(typeof(DifferentMessagesGenerator))]
        public void GetMessageType_EmptyMessagesOfConcreteTypes_ReturnMessagesTypes(byte[] message, NetworkMessages type)
        {
            // Arrange

            // Act
            var messageType = MessageHelper.GetMessageType(message);

            // Assert
            Assert.Equal(type, messageType);
        }

        [Theory]
        [ClassData(typeof(MessagesWithBytesGenerator))]
        public void ToByteArray_MessagesWithByteData_ReturnByteData(NetworkMessages type, byte[] data)
        {
            // Arrange
            var message = MessageHelper.GetMessage(type, data);

            // Act
            var messageData = MessageHelper.ToByteArray(message);

            // Assert
            Assert.Equal(data, messageData);
        }

        [Theory]
        [ClassData(typeof(MessagesWithStringGenerator))]
        public void ToString_MessageWithStringData_ReturnStringData(NetworkMessages type, string data)
        {
            // Arrange
            var message = MessageHelper.GetMessage(type, data);

            // Act
            var messageData = MessageHelper.ToString(message);

            // Assert
            Assert.Equal(data, messageData);
        }
    }

    public class NetworkMessagesTypesEnumerator : IEnumerable<NetworkMessages>
    {
        public IEnumerator<NetworkMessages> GetEnumerator() 
            => 
            Enum.GetValues(typeof(NetworkMessages))
                .Cast<NetworkMessages>()
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class DifferentMessagesGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var type in new NetworkMessagesTypesEnumerator())
            {
                yield return new object[] { MessageHelper.GetMessage(type), type };
                yield return new object[] { MessageHelper.GetMessage(type, new byte[] { 0xaa, 0xbb, 0xcc }), type };
                yield return new object[] { MessageHelper.GetMessage(type, "data"), type };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class MessagesWithBytesGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator() 
            => new NetworkMessagesTypesEnumerator()
                .Select(type => new object[] {type, new byte[] { 0xaa, 0xbb, 0xcc } })
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class MessagesWithStringGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator() 
            => new NetworkMessagesTypesEnumerator()
                .Select(type => new object[] { type, "data" })
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
