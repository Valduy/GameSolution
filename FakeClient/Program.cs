using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Connectors.MatchConnectors;
using Matches.Messages;
using Network;
using Network.Messages;

namespace FakeClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connector = new P2PMatchConnector();
            Console.WriteLine("Client started!");
            var result = await connector.ConnectAsync(args[0], int.Parse(args[1]));
            Console.WriteLine($"Role: {result.Role}");

            switch (result.Role)
            {
                case Role.Host:
                {
                    Console.WriteLine("Clients:");
                    foreach (var c in result.Clients)
                    {
                        Console.WriteLine($"{c.Ip}:{c.Port}");
                        await result.UdpClient.SendAsync(new byte[] { 1, 1, 1 }, 3, c.Ip, c.Port);
                        var data = await result.UdpClient.ReceiveAsync();

                        foreach (var b in data.Buffer)
                        {
                            Console.Write(b);
                        }
                    }
                    break;
                }
                case Role.Client:
                {
                    Console.WriteLine("Host");
                    var host = result.Clients.First();
                    Console.WriteLine($"{host.Ip}:{host.Port}");
                    await result.UdpClient.SendAsync(new byte[] { 1, 1, 1 }, 3, host.Ip, host.Port);
                    var data = await result.UdpClient.ReceiveAsync();

                    foreach (var b in data.Buffer)
                    {
                        Console.Write(b);
                    }
                    break;
                }
            }

            Console.ReadKey();
        }
    }
}
