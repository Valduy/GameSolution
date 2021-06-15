using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Matches;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Network;
using Network.Messages;

namespace FakeListenSession
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 1) throw new Exception("Ожидалось не более одного аргумента");
            var port = int.Parse(args[0]);

            var udpClient = new UdpClient(port);
            Console.WriteLine("Ожидание клиентов...");
            var clients = await WaitClients(udpClient, 2);
            Console.WriteLine("Клиентов достаточно, можно соединять...");

            var match = new ListenSessionMatch(clients, 120000, udpClient, GetLogger());
            Console.WriteLine("Fake listen session started!");
            await match.WorkAsync();
            Console.WriteLine("Completed!");
        }

        private static async Task<List<ClientEndPoints>> WaitClients(UdpClient udpClient, int count)
        {
            var clientsEndPoints = new List<ClientEndPoints>();

            while (clientsEndPoints.Count < 2)
            {
                if (udpClient.Available > 0)
                {
                    var result = await udpClient.ReceiveAsync();
                    var newClientEndPoints = CreateClientEndPoints(result.RemoteEndPoint, result.Buffer);

                    if (!clientsEndPoints.Any(endPoints => endPoints.IsSame(newClientEndPoints)))
                    {
                        Console.WriteLine($"Добавляю: {newClientEndPoints.PublicEndPoint}:{newClientEndPoints.PublicEndPoint.Port}");
                        clientsEndPoints.Add(newClientEndPoints);
                    }

                    Console.WriteLine($"Отвечаю: {result.RemoteEndPoint}.");
                    var message = MessageHelper.GetMessage(NetworkMessages.Info);
                    await udpClient.SendAsync(message, message.Length, result.RemoteEndPoint);
                }
            }

            return clientsEndPoints;
        }

        private static ClientEndPoints CreateClientEndPoints(IPEndPoint ip, byte[] received)
        {
            var data = MessageHelper.ToString(received);
            var privateEndPoint = JsonSerializer.Deserialize<ClientEndPoint>(data)
                                  ?? throw new ArgumentException("При десериализации конечной точки был получен null.");

            return new ClientEndPoints(
                ip.Address.ToString(),
                ip.Port,
                privateEndPoint.Ip,
                privateEndPoint.Port
            );
        }

        private static ILogger<ListenSessionMatch> GetLogger()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            return factory.CreateLogger<ListenSessionMatch>();
        }
    }
}
