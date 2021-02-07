using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Connectors.MatchConnectors.States;
using GameLoops;
using Matches.Messages;
using Network.Messages;

namespace Connectors.MatchConnectors
{
    public class P2PMatchConnector : IP2PMatchConnectorBase
    {
        private bool _isRun;
        private UdpClient _udpClient;
        private CancellationToken _token;

        internal string Ip { get; private set; }
        internal int Port { get; private set; }
        internal string ServerIp { get; private set; }
        internal int ServerPort { get; private set; }
        internal Role Role { get; set; }
        internal List<ClientEndPoints> PotentialEndPoints { get; set; }
        internal List<ClientEndPoint> RealEndPoints { get; set; }
        internal P2PMatchConnectorStateBase State { get; set; }

        public async Task<P2PConnectionResult> ConnectAsync(string serverIp, int serverPort, CancellationToken token = default)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            _token = token;
            InitUdpClient();
            State = new HelloMatchState(this);
            return await ConnectionLoopAsync();
        }

        internal async Task SendMessageAsync(byte[] message, string ip, int port)
            => await _udpClient.SendAsync(message, message.Length, ip, port);

        internal async Task SendMessageAsync(byte[] message, ClientEndPoint endPoint)
            => await SendMessageAsync(message, endPoint.Ip, endPoint.Port);

        internal async Task SendMessageAsync(byte[] message, IPEndPoint ip)
            => await _udpClient.SendAsync(message, message.Length, ip);

        internal void CompleteConnection()
        {
            _isRun = false;
        }

        private void InitUdpClient()
        {
            _udpClient = new UdpClient(0);
            Port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
            Ip = GetLocalIPAddress();
        }

        private async Task<P2PConnectionResult> ConnectionLoopAsync()
        {
            _isRun = true;

            while (_isRun)
            {
                if (_token.IsCancellationRequested)
                {
                    _udpClient.Dispose();
                    _udpClient = null;
                    _isRun = false;
                    return null;
                }

                await ReceiveAndSend();
                await Task.Delay(100, _token);
            }

            return new P2PConnectionResult(Role, _udpClient, RealEndPoints);
        }

        private async Task ReceiveAndSend()
        {
            if (_udpClient.Available > 0)
            {
                var received = await _udpClient.ReceiveAsync();
                State.ProcessMessage(received.RemoteEndPoint, received.Buffer);
            }

            await State.SendMessageAsync();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
