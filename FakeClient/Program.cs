using System;
using System.Collections.Generic;
using System.Linq;
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
            Console.WriteLine("Fake clientMessage started!");

            ConnectWithServer();
        }

        private static void ConnectWithServer()
        {
            var helloMessage = MessageHelper.GetMessage(NetworkMessages.Hello);
            IPEndPoint ip = null;
            Console.WriteLine("Let's wait server responce...");

            while (true)
            {
                if (_udpClient.Available > 0)
                {
                    var response = _udpClient.Receive(ref ip);

                    switch (MessageHelper.GetMessageType(response))
                    {
                        case NetworkMessages.Hello:
                            _udpClient.Send(helloMessage, helloMessage.Length, _serverIp, _serverPort);
                            break;
                        case NetworkMessages.Host:
                        {
                            var message = JsonSerializer.Deserialize<HostMessage>(MessageHelper.ToString(response));
                            Console.WriteLine("Server has answer!");
                            Console.WriteLine($"I am host!");
                            RunHostLogic(message);
                            return;
                        }
                        case NetworkMessages.Client:
                        {
                            var message = JsonSerializer.Deserialize<ClientMessage>(MessageHelper.ToString(response));
                            Console.WriteLine("Server has answer!");
                            Console.WriteLine($"I am clientMessage!");
                            RunClientLogic(message);
                            return;
                        }
                    }
                }

                _udpClient.Send(helloMessage, helloMessage.Length, _serverIp, _serverPort);
                Thread.Sleep(1000);
            }
        }

        private static void RunHostLogic(HostMessage hostMessage)
        {
            ConnectHostWithClients(hostMessage);

            var reactions = new Dictionary<string, string>()
            {
                {"w", "up"},
                {"d", "right"},
                {"s", "down"},
                {"a", "left"}
            };

            var connectMessage = MessageHelper.GetMessage(NetworkMessages.Connect);
            IPEndPoint ip = null;
            Console.WriteLine("Let's run as host!");

            while (true)
            {
                if (_udpClient.Available > 0)
                {
                    var message = _udpClient.Receive(ref ip);

                    switch (MessageHelper.GetMessageType(message))
                    {
                        //TODO: проверить, кто нам шлет сообщения
                        case NetworkMessages.Info:
                            var data = MessageHelper.ToString(message);
                            Console.WriteLine($"Message from clientMessage: {data}.");

                            if (reactions.TryGetValue(data, out var value))
                            {
                                var response = MessageHelper.GetMessage(NetworkMessages.Info, value);
                                _udpClient.Send(response, response.Length, ip);
                            }
                            break;
                        case NetworkMessages.Connect:
                            _udpClient.Send(connectMessage, connectMessage.Length, ip);
                            break;
                    }
                }
            }
        }

        private static void ConnectHostWithClients(HostMessage hostMessage)
        {
            var clients = new HashSet<ClientMessage>(hostMessage.Clients);
            var connectMessage = MessageHelper.GetMessage(NetworkMessages.Connect);
            IPEndPoint ip = null;

            while (clients.Any())
            {
                if (_udpClient.Available > 0)
                {
                    var message = _udpClient.Receive(ref ip);

                    if (MessageHelper.GetMessageType(message) == NetworkMessages.Connect)
                    {
                        var client = clients.First(o => o.Ip == ip.Address.ToString() && o.Port == ip.Port);
                        clients.Remove(client);
                        _udpClient.Send(connectMessage, connectMessage.Length, ip);
                    }

                    foreach (var c in clients)
                    {
                        _udpClient.Send(connectMessage, connectMessage.Length, c.Ip, c.Port);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private static void RunClientLogic(ClientMessage clientMessage)
        {
            ConnectClientWithHost(clientMessage);
            var connectMessage = MessageHelper.GetMessage(NetworkMessages.Connect);
            IPEndPoint ip = null;
            Console.WriteLine("Let's run as clientMessage!");

            while (true)
            {
                Console.WriteLine("Enter data...");
                var input = Console.ReadLine();
                var request = MessageHelper.GetMessage(NetworkMessages.Info, input);
                _udpClient.Send(request, request.Length, clientMessage.Ip, clientMessage.Port);

                if (_udpClient.Available > 0)
                {
                    var message = _udpClient.Receive(ref ip);

                    switch (MessageHelper.GetMessageType(message))
                    {
                        //TODO: проверить, кто нам шлет сообщения
                        case NetworkMessages.Info:
                            var data = MessageHelper.ToString(message);
                            Console.WriteLine($"Message from server: {data}.");
                            break;
                        case NetworkMessages.Connect:
                            _udpClient.Send(connectMessage, connectMessage.Length, ip);
                            break;
                    }
                }
            }
        }

        private static void ConnectClientWithHost(ClientMessage clientMessage)
        {
            var connectMessage = MessageHelper.GetMessage(NetworkMessages.Connect);
            IPEndPoint ip = null;

            while (true)
            {
                if (_udpClient.Available > 0)
                {
                    var message = _udpClient.Receive(ref ip);

                    if (MessageHelper.GetMessageType(message) == NetworkMessages.Connect)
                    {
                        if (ip.Address.ToString() == clientMessage.Ip && ip.Port == clientMessage.Port)
                        {
                            _udpClient.Send(connectMessage, connectMessage.Length, ip);
                            return;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
