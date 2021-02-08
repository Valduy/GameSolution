using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
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
                    }
                    break;
                }
                case Role.Client:
                {
                    Console.WriteLine("Host:");
                    var host = result.Clients.First();
                    Console.WriteLine($"{host.Ip}:{host.Port}");
                    break;
                }
            }

            var readTask = Task.Run(() =>
            {
                IPEndPoint ip = null;

                while (true)
                {
                    if (result.UdpClient.Available > 0)
                    {
                        lock (result.UdpClient)
                        {
                            
                            var bytes = result.UdpClient.Receive(ref ip);
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
                    var bytes = Encoding.ASCII.GetBytes(message);
                    
                    lock (result.UdpClient)
                    {
                        foreach (var c in result.Clients)
                        {
                            result.UdpClient.Send(bytes, bytes.Length, c.Ip, c.Port);
                        }
                    }
                }
            });

            Task.WaitAll(readTask, writeTask);
        }
    }
}
