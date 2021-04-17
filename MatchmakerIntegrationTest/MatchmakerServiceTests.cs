using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Matchmaker.Services;
using Network;
using Network.Messages;
using System.Text.Json;
using Matchmaker.Factories.Implementations;
using Orderers.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace Matchmaker.Tests.Integration
{
    public class MatchmakerServiceFixture : IDisposable
    {
        public MatchmakerService Matchmaker { get; }
        public UdpClient UdpClient1 { get; }
        public UdpClient UdpClient2 { get; }

        public int Delay => 1000;
        public int MatchPort { get; set; }
        public int Port1 { get; }
        public int Port2 { get; }
        public string UserId1 => "userId1";
        public string UserId2 => "userId2";

        public MatchmakerServiceFixture()
        {
            Matchmaker = new MatchmakerService(new ListenMatchFactory());
            UdpClient1 = new UdpClient(0);
            UdpClient2 = new UdpClient(0);
            Port1 = ((IPEndPoint)UdpClient1.Client.LocalEndPoint).Port;
            Port2 = ((IPEndPoint)UdpClient2.Client.LocalEndPoint).Port;
        }

        public void Dispose()
        {
            Matchmaker.Dispose();
            UdpClient1.Close();
            UdpClient2.Close();
        }
    }

    [TestCaseOrderer("Orderers.PriorityOrderer", "Orderers")]
    public class MatchmakerServiceTests : IClassFixture<MatchmakerServiceFixture>
    {
        private readonly MatchmakerServiceFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        public MatchmakerServiceTests(MatchmakerServiceFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
        }

        [Fact, TestPriority(0)]
        public void GetStatus_UserId_ReturnUserStatusAbsent()
        {
            // Arrange
            var userId = "userId";

            // Act
            var status = _fixture.Matchmaker.GetStatus(userId);

            // Assert
            Assert.Equal(UserStatus.Absent, status);
        }

        [Fact, TestPriority(1)]
        public void Enqueue_UserId_ReturnUserStatusWait()
        {
            // Arrange
            var publicIp = IPAddress.Loopback.ToString();
            var privateIp = NetworkHelper.GetLocalIPAddress();
            var publicPort = 0;
            var privatePort = _fixture.Port1;
            var endPoints1 = new ClientEndPoints(publicIp, publicPort, privateIp, privatePort);

            // Act
            _fixture.Matchmaker.Enqueue(_fixture.UserId1, endPoints1);
            var status1 = _fixture.Matchmaker.GetStatus(_fixture.UserId1);

            // Assert
            Assert.Equal(UserStatus.Wait, status1);
        }

        [Fact, TestPriority(2)]
        public async Task GetStatus_UsersId_ReturnUsersStatusesConnected()
        {
            // Arrange
            var publicIp = IPAddress.Loopback.ToString();
            var privateIp = NetworkHelper.GetLocalIPAddress();
            var publicPort = 0;
            var privatePort = _fixture.Port2;
            var endPoints2 = new ClientEndPoints(publicIp, publicPort, privateIp, privatePort);

            // Act
            _fixture.Matchmaker.Enqueue(_fixture.UserId2, endPoints2);
            await Task.Delay(_fixture.Delay);
            var status1 = _fixture.Matchmaker.GetStatus(_fixture.UserId1);
            var status2 = _fixture.Matchmaker.GetStatus(_fixture.UserId2);

            // Assert
            Assert.Equal(UserStatus.Connected, status1);
            Assert.Equal(UserStatus.Connected, status2);
        }

        [Fact, TestPriority(3)]
        public void GetMatch_UsersId_ReturnSameMatchPort()
        {
            // Arrange

            // Act
            var port1 = _fixture.Matchmaker.GetMatch(_fixture.UserId1);
            var port2 = _fixture.Matchmaker.GetMatch(_fixture.UserId2);
            _fixture.MatchPort = port1!.Value;

            // Assert
            Assert.NotEqual(0, port1);
            Assert.Equal(port1, port2);
        }

        [Fact, TestPriority(4)]
        public async Task MatchWork_InitialMessagesWithPrivateEndPoints_ReturnConnectionMessages()
        {
            // Arrange
            var message1 = GetMatchMessage(NetworkHelper.GetLocalIPAddress(), _fixture.Port1);
            var message2 = GetMatchMessage(NetworkHelper.GetLocalIPAddress(), _fixture.Port2);
            var privateEndPoint1 = new ClientEndPoint(NetworkHelper.GetLocalIPAddress(), _fixture.Port1);
            var privateEndPoint2 = new ClientEndPoint(NetworkHelper.GetLocalIPAddress(), _fixture.Port2);

            // Act
            var connectionTask1 = Task.Run(
                async () => await SendAndReceiveForConnectionMessage(
                    _fixture.UdpClient1, 
                    message1, 
                    _fixture.MatchPort));

            var connectionTask2 = Task.Run(
                async () => await SendAndReceiveForConnectionMessage(
                    _fixture.UdpClient2, 
                    message2, 
                    _fixture.MatchPort));

            var connectionMessage1 = await connectionTask1;
            var connectionMessage2 = await connectionTask2;

            // Assert
            Assert.True(connectionMessage1.Clients.First().PrivateEndPoint.IsSame(privateEndPoint2));
            Assert.True(connectionMessage2.Clients.First().PrivateEndPoint.IsSame(privateEndPoint1));
        }

        private byte[] GetMatchMessage(string ip, int port) 
            => MessageHelper.GetMessage(
                NetworkMessages.Hello, 
                JsonSerializer.Serialize(new ClientEndPoint(ip, port)));

        private async Task<ConnectionMessage> SendAndReceiveForConnectionMessage(UdpClient client, byte[] request, int port)
        {
            byte[] response = null;

            do
            {
                await client.SendAsync(request, request.Length, IPAddress.Loopback.ToString(), port);

                while (client.Available > 0)
                {
                    response = (await client.ReceiveAsync()).Buffer;
                }
            } while (response == null || MessageHelper.GetMessageType(response) != NetworkMessages.Initial);

            return JsonSerializer.Deserialize<ConnectionMessage>(MessageHelper.ToString(response));
        }
    }
}
