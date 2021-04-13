using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Matches.States;
using Network.Messages;

namespace Matches
{
    public class ListenSessionMatch : IMatch, IDisposable
    {
        private UdpClient _udpClient;
        private CancellationToken _token;
        private List<ClientEndPoints> _clients;
        private ClientEndPoints _host;
        private Stopwatch _timer;
        
        public int Port { get; private set; }
        public int PlayersCount { get; private set; }
        public long TimeForStarting { get; private set; }

        public IReadOnlyList<ClientEndPoints> Clients => _clients;
        public ClientEndPoints Host => _host;

        internal ListenSessionStateBase State { get; set; }

        public event Action<IMatch> MatchStarted;

        public async Task WorkAsync(int playersCount, CancellationToken token = default)
            => await WorkAsync(playersCount, 30000, 0, token);

        public async Task WorkAsync(int playersCount, long timeForStarting, CancellationToken token = default)
            => await WorkAsync(playersCount, timeForStarting, 0, token);

        public async Task WorkAsync(int playersCount, long timeForStarting, int port, CancellationToken token = default)
        {
            PlayersCount = playersCount;
            TimeForStarting = timeForStarting;
            _token = token;

            _udpClient = new UdpClient(port);
            Port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;

            State = new WaitClientState(this);
            _host = null;
            _clients = new List<ClientEndPoints>();
            _timer = new Stopwatch();
            _timer.Start();

            await ConnectionLoopAsync();
        }

        internal void AddClient(ClientEndPoints client) => _clients.Add(client);

        internal void NotifyThatStarted() => MatchStarted?.Invoke(this);

        internal void ChooseHost(ClientEndPoints host)
        {
            var client = _clients.FirstOrDefault(o => o.Equals(host)) 
                         ?? throw new ArgumentException("Такой игрок отсутствует среди клиентов.");
            _clients.Remove(client);
            _host = client;
        }

        internal async Task SendMessageAsync(byte[] message, IPEndPoint ip)
            => await _udpClient.SendAsync(message, message.Length, ip);

        private async Task ConnectionLoopAsync()
        {
            while (_timer.ElapsedMilliseconds < TimeForStarting && !_token.IsCancellationRequested)
            {
                await ReceiveAndAnswerAsync();
            }

            _udpClient.Close();
        }

        private async Task ReceiveAndAnswerAsync()
        {
            if (_udpClient.Available > 0)
            {
                var received = await _udpClient.ReceiveAsync();
                await State.ProcessMessageAsync(received.RemoteEndPoint, received.Buffer);
            }
        }

        public void Dispose()
        {
            _udpClient.Close();
            _udpClient?.Dispose();
        }
    }
}
