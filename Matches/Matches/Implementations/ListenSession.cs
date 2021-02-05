using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using GameLoops;
using Matches.Messages;
using Network;

namespace Matches
{
    public class ListenSession : MatchBase, IDisposable
    {
        private readonly UdpClient _udpClient;

        private FixedFpsGameLoop _connectionLoop;
        private IPEndPoint _host;
        private Stopwatch _timer;
        private string _hostMessage;
        private string _clientMessage;

        public ListenSession(long timeForStarting = 30000)
            : base(timeForStarting)
        {
            _udpClient = new UdpClient(0);
            Port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
        }

        public ListenSession(long timeForStarting = 30000, int port = 0) 
            : base(timeForStarting, port)
        {
            _udpClient = new UdpClient(port);
        }

        public void Dispose()
        {
            _udpClient?.Dispose();
        }

        public override void Start()
        {
            base.Start();
            ConnectPlayers();
        }

        public override void Stop()
        {
            base.Stop();
            _udpClient?.Dispose();
            _connectionLoop?.Stop();
            _timer?.Stop();
        }

        /// <summary>
        /// Метод обеспечивает соединение игроков.
        /// </summary>
        private void ConnectPlayers()
        {
            _timer = new Stopwatch();
            _connectionLoop = new FixedFpsGameLoop(ConnectionLoopFrame, 30);
            _timer.Start();
            _connectionLoop.Start();
        }

        private async void ConnectionLoopFrame(double dt)
        {
            if (_timer.ElapsedMilliseconds >= TimeForStarting)
            {
                Stop();
                return;
            }

            await ReceiveAndAnswer();
        }

        private async Task ReceiveAndAnswer()
        {
            if (_udpClient.Available > 0)
            {
                var player = await _udpClient.ReceiveAsync();

                if (_host == null)
                {
                    _host = player.RemoteEndPoint;
                    _hostMessage = JsonSerializer.Serialize(new ConnectionMessage { Role = Role.Host });
                    _clientMessage = JsonSerializer.Serialize(new ConnectionMessage
                    {
                        Role = Role.Client,
                        Ip = _host.Address.ToString(),
                        Port = _host.Port
                    });
                }

                var message = Equals(player.RemoteEndPoint, _host)
                    ? MessageHelper.GetMessage(NetworkMessages.Hello, _hostMessage)
                    : MessageHelper.GetMessage(NetworkMessages.Hello, _clientMessage);

                await _udpClient.SendAsync(message, message.Length, player.RemoteEndPoint);
            }
        }
    }
}
