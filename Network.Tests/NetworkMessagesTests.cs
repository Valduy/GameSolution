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
        [ClassData(typeof(TestMessagesData))]
        public void WriteAndReadDataFromMessageTheory(NetworkMessages type, byte[] data)
        {
            var message = MessageHelper.GetMessage(type, data);
            Assert.Equal(type, MessageHelper.GetMessageType(message));
            Assert.Equal(data, MessageHelper.FromMessage(message));
        }
    }

    public class TestMessagesData : IEnumerable<object[]>
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
}
