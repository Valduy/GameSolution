using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Matches.Messages;
using Matches.States;
using Network.Messages;

namespace Matches
{
    public class ListenSessionMatch : MatchBase, IDisposable
    {
        private readonly UdpClient _udpClient;

        private CancellationToken _token;
        private List<ClientEndPoints> _clients;
        private ClientEndPoints _host;
        private Stopwatch _timer;

        public IReadOnlyList<ClientEndPoints> Clients => _clients;
        public ClientEndPoints Host => _host;
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

        public override async Task WorkAsync(CancellationToken token = default)
        {
            _token = token;
            State = new WaitClientState(this);
            _clients = new List<ClientEndPoints>();
            _timer = new Stopwatch();
            _timer.Start();
            await ConnectionLoopAsync();
        }

        internal void AddClient(ClientEndPoints client) => _clients.Add(client);

        internal void ChooseHost(ClientEndPoints host)
        {
            var client = _clients.FirstOrDefault(o => o.Equals(host));
            if (client == null) throw new ArgumentException();
            _clients.Remove(client);
            _host = client;
        }

        internal async Task SendMessageAsync(byte[] message, IPEndPoint ip)
            => await _udpClient.SendAsync(message, message.Length, ip);

        private async Task ConnectionLoopAsync()
        {
            while (_timer.ElapsedMilliseconds < TimeForStarting)
            {
                if (_token.IsCancellationRequested)
                {
                    break;
                }

                await ReceiveAndAnswer();
                await Task.Delay(100, _token);
            }
        }

        private async Task ReceiveAndAnswer()
        {
            if (_udpClient.Available > 0)
            {
                var received = await _udpClient.ReceiveAsync();
                await State.ProcessMessageAsync(received.RemoteEndPoint, received.Buffer);
            }
        }

        public void Dispose()
        {
            _udpClient?.Dispose();
        }
    }
}
