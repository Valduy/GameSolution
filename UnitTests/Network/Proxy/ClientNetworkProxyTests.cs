using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ECS.Core;
using Network;
using Network.Proxy;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.Network.Proxy
{
    public class TestNotComponentClass
    {
        public bool W { get; set; }
        public bool A { get; set; }
        public bool S { get; set; }
        public bool D { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TestComponentClass : ComponentBase
    {
        [JsonProperty]
        public bool W { get; set; }
        [JsonProperty]
        public bool A { get; set; }
        [JsonProperty]
        public bool S { get; set; }
        [JsonProperty]
        public bool D { get; set; }
    }

    public class ClientNetworkProxyFixture : IDisposable
    {
        public UdpClient HostUdpClient { get; }
        public UdpClient ClientUdpClient { get; }
        public ClientNetworkProxy ClientProxy { get; }

        public ClientNetworkProxyFixture()
        {
            HostUdpClient = new UdpClient(0);
            ClientUdpClient = new UdpClient(0);
            ClientProxy = new ClientNetworkProxy(ClientUdpClient,
                new IPEndPoint(IPAddress.Loopback, HostUdpClient.GetPort()));
            ClientProxy.Start();
        }

        public void Dispose()
        {
            HostUdpClient?.Dispose();
            ClientUdpClient?.Dispose();
            ClientProxy?.Dispose();
        }
    }

    public class ClientNetworkProxyTests : IClassFixture<ClientNetworkProxyFixture>
    {
        private readonly ClientNetworkProxyFixture _fixture;

        public ClientNetworkProxyTests(ClientNetworkProxyFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [ClassData(typeof(MessagesGenerator))]
        public void WriteToWriteBuffer_Messages_ReceiveWrittenMessages(byte[] message)
        {
            IPEndPoint endPoint = null;

            _fixture.ClientProxy.WriteBuffer.Write(message);
            var received = _fixture.HostUdpClient.Receive(ref endPoint);
            var data = PacketHelper.GetData(received);

            Assert.Equal(message, data);
        }

        [Theory]
        [ClassData(typeof(MessagesGenerator))]
        public void ReadFromReadBuffer_Messages_SendedMessages(byte[] message)
        {
            var packet = PacketHelper.CreatePacket(0, message);

            _fixture.HostUdpClient.Send(packet, packet.Length,
                new IPEndPoint(IPAddress.Loopback, _fixture.ClientUdpClient.GetPort()));
            while (_fixture.ClientProxy.ReadBuffer.IsEmpty);
            var received = _fixture.ClientProxy.ReadBuffer.ReadLast();

            Assert.Equal(message, received);
        }
    }

    public class MessagesGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new byte[] {1, 2, 3, 4}
            };

            var json = JsonConvert.SerializeObject(new TestNotComponentClass
            {
                W = true,
                A = true,
                S = true,
                D = true,
            });

            yield return new object[]
            {
                Encoding.ASCII.GetBytes(json)
            };

            var entity = new Entity();
            var component = new TestComponentClass();
            entity.Add(component);
            json = JsonConvert.SerializeObject(component);

            yield return new object[]
            {
                Encoding.ASCII.GetBytes(json)
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
