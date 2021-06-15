using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Connectors.HolePuncher;
using Connectors.MatchConnectors;

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
            Console.WriteLine($"Role: {result.Role}");

            var holePuncher = new HolePuncher();
            var endPoints = await holePuncher.ConnectAsync(udpClient, result.SessionId, result.Clients);

            Console.WriteLine($"My role: {result.Role}");
            Console.WriteLine("Clients:");

            foreach (var ep in endPoints)
            {
                Console.WriteLine($"{ep}");
            }

            var readTask = Task.Run(() =>
            {
                IPEndPoint ip = null;

                while (true)
                {
                    if (udpClient.Available > 0)
                    {
                        lock (udpClient)
                        {

                            var bytes = udpClient.Receive(ref ip);
                            var message = Encoding.ASCII.GetString(bytes);
                            Console.WriteLine("Received message:");
                            Console.WriteLine(message);
                            Console.WriteLine("Enter message:");
                        }
                    }
                }
            });

            var writeTask = Task.Run(() =>
            {
                while (true)
                {
                    var message = Console.ReadLine();
                    var bytes = Encoding.ASCII.GetBytes(message ?? string.Empty);

                    lock (udpClient)
                    {
                        foreach (var ep in endPoints)
                        {
                            udpClient.Send(bytes, bytes.Length, ep);
                        }
                    }
                }
            });

            Task.WaitAll(readTask, writeTask);
        }
    }
}
