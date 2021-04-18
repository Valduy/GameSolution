using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Connectors.MatchConnectors;
using Matches;
using Network;
using Network.Messages;
using Xunit;

namespace Connectors.Tests.Integration
{
    public class MatchConnectorTests
    {
        [Fact]
        public async Task ConnectAsyncTest()
        {
            // Arrange
            var udpClient1 = new UdpClient(0);
            var udpClient2 = new UdpClient(0);

            var publicEndPoint1 = new ClientEndPoint(IPAddress.Loopback.ToString(), 0);
            var privateEndPoint1 = new ClientEndPoint(NetworkHelper.GetLocalIPAddress(), udpClient1.GetPort());
            var endPoints1 = new ClientEndPoints(publicEndPoint1, privateEndPoint1);

            var publicEndPoint2 = new ClientEndPoint(IPAddress.Loopback.ToString(), 0);
            var privateEndPoint2 = new ClientEndPoint(NetworkHelper.GetLocalIPAddress(), udpClient2.GetPort());
            var endPoints2 = new ClientEndPoints(publicEndPoint2, privateEndPoint2);

            var connector1 = new MatchConnector();
            var connector2 = new MatchConnector();

            var match = new ListenSessionMatch(new []{endPoints1, endPoints2});

            // Act
            var tokenSource = new CancellationTokenSource();
            _ = Task.Run(async () => await match.WorkAsync(tokenSource.Token), tokenSource.Token);

            var connectorTask1 = Task.Run(
                async () => await connector1.ConnectAsync(
                    udpClient1, 
                    IPAddress.Loopback.ToString(),
                    match.Port));

            var connectorTask2 = Task.Run(
                async () => await connector2.ConnectAsync(
                    udpClient2,
                    IPAddress.Loopback.ToString(),
                    match.Port));

            var connectionMessage1 = await connectorTask1;
            var connectionMessage2 = await connectorTask2;
            tokenSource.Cancel();

            // Assert
            Assert.True(connectionMessage1.Clients.First().PrivateEndPoint.IsSame(privateEndPoint2));
            Assert.True(connectionMessage2.Clients.First().PrivateEndPoint.IsSame(privateEndPoint1));
        }
    }
}
