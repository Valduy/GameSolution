using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameLoops;
using Matches.Matches.States;

namespace Matches.Matches
{
    public class ListenSessionMatch : MatchBase
    {
        private readonly UdpClient _udpClient;

        private List<IPEndPoint> _clients;
        private IPEndPoint _host;
        private FixedFpsGameLoop _connectionLoop;
        private Stopwatch _timer;

        public IReadOnlyList<IPEndPoint> Clients => _clients;
        public IPEndPoint Host => _host;
        internal ListenSessionStateBase State { get; set; }

        public ListenSessionMatch(int playersCount, long timeForStarting) 
            : this(playersCount, timeForStarting, 0)
        {
        }

        public ListenSessionMatch(int playersCount, long timeForStarting, int port) 
            : base(playersCount, timeForStarting, port)
        {
            _udpClient = new UdpClient(port);
            Port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
        }

        public override void Start()
        {
            base.Start();
            State = new WaitClientState(this);
            _clients = new List<IPEndPoint>();
            _timer = new Stopwatch();
            _connectionLoop = new FixedFpsGameLoop(ConnectionLoopFrame, 30);
            _timer.Start();
            _connectionLoop.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _connectionLoop.Stop();
        }

        internal void AddClient(IPEndPoint client) => _clients.Add(client);

        internal void ChooseHost(IPEndPoint host)
        {
            var client = _clients.FirstOrDefault(o => o.Equals(host));
            if (client == null) throw new ArgumentException();
            _clients.Remove(client);
            _host = client;
        }

        internal async Task SendMessageAsync(byte[] message, IPEndPoint ip)
            => await _udpClient.SendAsync(message, message.Length, ip);

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
                var received = await _udpClient.ReceiveAsync();
                await State.ProcessMessageAsync(received.RemoteEndPoint, received.Buffer);
            }
        }
    }
}
