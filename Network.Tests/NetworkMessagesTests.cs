using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Network.Tests
{
    public class NetworkMessagesTests
    {
        [Theory]
        [InlineData(NetworkMessages.Hello)]
        [InlineData(NetworkMessages.Info)]
        [InlineData(NetworkMessages.Bye)]
        public void MessageTypeReadAndWriteTheory(NetworkMessages type)
        {
            var message = MessageHelper.GetMessage(type);
            Assert.Equal(type, MessageHelper.GetMessageType(message));
        }

        [Theory]
        [ClassData(typeof(TestMessagesByteData))]
        public void WriteAndReadBytesFromMessageTheory(NetworkMessages type, byte[] data)
        {
            var message = MessageHelper.GetMessage(type, data);
            Assert.Equal(type, MessageHelper.GetMessageType(message));
            Assert.Equal(data, MessageHelper.ToByteArray(message));
        }

        [Theory]
        [ClassData(typeof(TestMessagesStringData))]
        public void WriteAndReadStringFromMessageTheory(NetworkMessages type, string data)
        {
            var message = MessageHelper.GetMessage(type, data);
            Assert.Equal(type, MessageHelper.GetMessageType(message));
            Assert.Equal(data, MessageHelper.ToString(message));
        }
    }

    public class TestMessagesByteData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            byte[] data = { 0xaa, 0xbb, 0xcc };
            yield return new object[] { NetworkMessages.Hello, data };
            yield return new object[] { NetworkMessages.Info, data };
            yield return new object[] { NetworkMessages.Bye, data };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TestMessagesStringData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var data = "It's just network message...";
            yield return new object[] { NetworkMessages.Hello, data };
            yield return new object[] { NetworkMessages.Info, data };
            yield return new object[] { NetworkMessages.Bye, data };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
