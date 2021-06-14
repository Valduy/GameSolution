using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Connectors;
using Connectors.MatchConnectors;
using Matches;
using Network;
using Network.Messages;
using Xunit;

namespace IntegrationTests.Connectors.HolePuncher
{
    public class HolePuncherTests
    {
        [Fact]
        public async Task ConnectAsyncTest()
        {
            var holePuncher1 = new global::Connectors.HolePuncher.HolePuncher();
            var holePuncher2 = new global::Connectors.HolePuncher.HolePuncher();

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

            var match = new ListenSessionMatch(new[] { endPoints1, endPoints2 });
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

            var holePunchingTask1 = Task.Run(
                async () => await holePuncher1.ConnectAsync(udpClient1, 1, connectionMessage1.Clients));
            var holePunchingTask2 = Task.Run(
                async () => await holePuncher2.ConnectAsync(udpClient2, 1, connectionMessage2.Clients));

            var clients1 = await holePunchingTask1;
            var clients2 = await holePunchingTask2;

            Assert.Equal(clients1.First().Port, udpClient2.GetPort());
            Assert.Equal(clients2.First().Port, udpClient1.GetPort());
        }

        [Fact]
        public async Task ConnectionFailureTest()
        {
            var holePuncher1 = new global::Connectors.HolePuncher.HolePuncher();

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

            var match = new ListenSessionMatch(new[] { endPoints1, endPoints2 });
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
            _ = await connectorTask2;
            tokenSource.Cancel();

            var holePunchingTask1 = holePuncher1.ConnectAsync(udpClient1, 1, connectionMessage1.Clients);

            Assert.Throws<ConnectorException>(() =>
            {
                try
                {
                    var _ = holePunchingTask1.Result;
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }
    }
}
