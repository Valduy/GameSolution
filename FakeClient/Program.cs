using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Matches.Messages;
using Network;

namespace FakeClient
{
    class Program
    {
        private static UdpClient _udpClient;
        private static string _serverIp;
        private static int _serverPort;

        static void Main(string[] args)
        {
            _serverIp = args[0];
            _serverPort = int.Parse(args[1]);
            _udpClient = new UdpClient(0);
            Console.WriteLine("Fake client started!");

            var message = TryConnect();
            

            switch (message.Role)
            {
                case Role.Host:
                    RunHostLogic();
                    break;
                case Role.Client:
                    RunClientLogic(message);
                    break;
            }
        }

        private static ConnectionMessage TryConnect()
        {
            var helloMessage = MessageHelper.GetMessage(NetworkMessages.Hello);

            IPEndPoint ip = null;
            ConnectionMessage message = null;
            Console.WriteLine("Let's wait server responce...");

            while (true)
            {
                if (_udpClient.Available > 0)
                {
                    var response = _udpClient.Receive(ref ip);

                    if (MessageHelper.GetMessageType(response) == NetworkMessages.Hello)
                    {
                        message = JsonSerializer.Deserialize<ConnectionMessage>(MessageHelper.ToString(response));
                        Console.WriteLine("Server has answer!");
                        Console.WriteLine($"My role is {message.Role}!");
                        var buffer = MessageHelper.GetMessage(NetworkMessages.Info);
                        _udpClient.Send(buffer, buffer.Length, _serverIp, _serverPort);
                        break;
                    }
                }

                _udpClient.Send(helloMessage, helloMessage.Length, _serverIp, _serverPort);
                Thread.Sleep(1000);
            }

            return message;
        }

        private static void RunHostLogic()
        {
            var reactions = new Dictionary<string, string>()
            {
                {"w", "up"},
                {"d", "right"},
                {"s", "down"},
                {"a", "left"}
            };

            IPEndPoint ip = null;
            Console.WriteLine("Let's run as host!");

            while (true)
            {
                if (_udpClient.Available > 0)
                {
                    var message = _udpClient.Receive(ref ip);

                    if (MessageHelper.GetMessageType(message) == NetworkMessages.Info)
                    {
                        var data = MessageHelper.ToString(message);
                        Console.WriteLine($"Message from client: {data}.");

                        if (reactions.TryGetValue(data, out var value))
                        {
                            var response = MessageHelper.GetMessage(NetworkMessages.Info, value);
                            _udpClient.Send(response, response.Length, ip);
                        }
                    }
                }
            }
        }

        private static void RunClientLogic(ConnectionMessage connection)
        {
            IPEndPoint ip = null;
            Console.WriteLine("Let's run as client!");

            while (true)
            {
                Console.WriteLine("Enter data...");
                var input = Console.ReadLine();
                var request = MessageHelper.GetMessage(NetworkMessages.Info, input);
                _udpClient.Send(request, request.Length, connection.Ip, connection.Port);

                if (_udpClient.Available > 0)
                {
                    var message = _udpClient.Receive(ref ip);

                    if (MessageHelper.GetMessageType(message) == NetworkMessages.Info)
                    {
                        var data = MessageHelper.ToString(message);
                        Console.WriteLine($"Message from server: {data}.");
                    }
                }
            }
        }
    }
}
