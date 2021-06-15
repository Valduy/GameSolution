using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Connectors.HolePuncher;
using Connectors.MatchConnectors;
using Network.Messages;
using Network.Proxy;

namespace FakeClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connector = new MatchConnector();
            var udpClient = new UdpClient(0);
            Console.WriteLine("Client started!");
            var result = await connector.ConnectAsync(udpClient, args[0], int.Parse(args[1]));
            // Нивелируем наличие нескольких адаптеров.
            result.Clients[0].PrivateEndPoint.Ip = args[0];
            Console.WriteLine($"Role: {result.Role}");

            var holePuncher = new HolePuncher();
            var endPoints = await holePuncher.ConnectAsync(udpClient, result.SessionId, result.Clients);

            Console.WriteLine($"My role: {result.Role}");
            Console.WriteLine("Clients:");

            foreach (var ep in endPoints)
            {
                Console.WriteLine($"{ep}");
            }

            switch (result.Role)
            {
                case Role.Host:
                {
                    var hostProxy = new HostNetworkProxy(udpClient, endPoints);
                    hostProxy.Start();

                    while (true)
                    {
                        hostProxy.GetWriteBuffer(endPoints[0]).Write(Encoding.ASCII.GetBytes("From host!"));
                        var buffer = hostProxy.GetReadBuffer(endPoints[0]);

                        if (!buffer.IsEmpty)
                        {
                            Console.WriteLine(Encoding.ASCII.GetString(buffer.ReadLast()));
                        }

                        await Task.Delay(1000);
                    }
                    break;
                }
                case Role.Client:
                {
                    var clientProxy = new ClientNetworkProxy(udpClient, endPoints[0]);
                    clientProxy.Start();

                    while (true)
                    {
                        clientProxy.WriteBuffer.Write(Encoding.ASCII.GetBytes("From client!"));

                        if (!clientProxy.ReadBuffer.IsEmpty)
                        {
                            Console.WriteLine(Encoding.ASCII.GetString(clientProxy.ReadBuffer.ReadLast()));
                        }

                        await Task.Delay(1000);
                    }
                    break;
                }
            }
        }
    }
}
