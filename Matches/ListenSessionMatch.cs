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
        
        public IEnumerable<ClientEndPoints> ExpectedPlayers { get; }
        public int Port { get; }
        public long TimeForStarting { get; }


        public IReadOnlyList<ClientEndPoints> Clients => _clients;
        public ClientEndPoints Host => _host;

        internal ListenSessionStateBase State { get; set; }


        public event Action<IMatch> MatchStarted;

        public ListenSessionMatch(IEnumerable<ClientEndPoints> playersEndPoints) 
            : this(playersEndPoints, 0)
        { }

        public ListenSessionMatch(IEnumerable<ClientEndPoints> playersEndPoints, int port) 
            : this(playersEndPoints, 30000, port)
        { }

        public ListenSessionMatch(IEnumerable<ClientEndPoints> playersEndPoints, long timeForStarting) 
            : this(playersEndPoints, timeForStarting, 0)
        { }

        public ListenSessionMatch(IEnumerable<ClientEndPoints> playersEndPoints, long timeForStarting, int port)
        {
            ExpectedPlayers = playersEndPoints;
            TimeForStarting = timeForStarting;
            _udpClient = new UdpClient(Port);
            Port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
        }

        public async Task WorkAsync(CancellationToken token = default)
        {
            _token = token;
            _host = null;
            _clients = new List<ClientEndPoints>();
            _timer = new Stopwatch();
            _timer.Start();
            State = new WaitClientState(this);

            await ConnectionLoopAsync();
        }

        internal void AddClient(ClientEndPoints client) => _clients.Add(client);

        internal void NotifyThatStarted() => MatchStarted?.Invoke(this);

        internal void ChooseHost(ClientEndPoints host)
        {
            var client = _clients.FirstOrDefault(o => o.IsSame(host)) 
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
            _udpClient.Dispose();
        }
    }
}
