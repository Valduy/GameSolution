using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Matches.States;
using Microsoft.Extensions.Logging;
using Network;
using Network.Messages;

namespace Matches
{
    public class ListenSessionMatch : IMatch
    {
        private static uint _lastSessionId;

        private readonly object _locker = new object();
        private readonly ILogger<ListenSessionMatch> _logger;
        private readonly UdpClient _udpClient;

        private CancellationToken _cancellationToken;
        private List<ClientEndPoints> _clients;
        private ClientEndPoints _host;
        private Stopwatch _timer;
        
        public IEnumerable<ClientEndPoints> ExpectedPlayers { get; }
        public int Port { get; }
        public long TimeForStarting { get; }

        public uint SessionId { get; set; }
        public IReadOnlyList<ClientEndPoints> Clients => _clients;
        public ClientEndPoints Host => _host;
        
        internal ListenSessionStateBase State { get; set; }

        private uint NextSessionId
        {
            get
            {
                lock (_locker)
                {
                    unchecked
                    {
                        return _lastSessionId++;
                    }
                }
            }
        }

        public event Action<IMatch> MatchStarted;

        public ListenSessionMatch(
            IEnumerable<ClientEndPoints> playersEndPoints, 
            ILogger<ListenSessionMatch> logger = null) 
            : this(playersEndPoints, 0, logger)
        { }

        public ListenSessionMatch(
            IEnumerable<ClientEndPoints> playersEndPoints, 
            int port, 
            ILogger<ListenSessionMatch> logger = null) 
            : this(playersEndPoints, 30000, port, logger)
        { }

        public ListenSessionMatch(
            IEnumerable<ClientEndPoints> playersEndPoints, 
            long timeForStarting, 
            ILogger<ListenSessionMatch> logger = null) 
            : this(playersEndPoints, timeForStarting, 0, logger)
        { }

        public ListenSessionMatch(
            IEnumerable<ClientEndPoints> playersEndPoints,
            long timeForStarting,
            int port,
            ILogger<ListenSessionMatch> logger = null)
            : this(playersEndPoints, timeForStarting, new UdpClient(port), logger)
        { }
        
        public ListenSessionMatch(
            IEnumerable<ClientEndPoints> playersEndPoints, 
            long timeForStarting, 
            UdpClient udpClient, 
            ILogger<ListenSessionMatch> logger = null)
        {
            SessionId = NextSessionId;
            _logger = logger;
            ExpectedPlayers = playersEndPoints;
            TimeForStarting = timeForStarting;
            _udpClient = udpClient;
            Port = _udpClient.GetPort();
        }

        public async Task WorkAsync(CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            _host = null;
            _clients = new List<ClientEndPoints>();
            _timer = new Stopwatch();
            _timer.Start();
            State = new WaitClientState(this);
            await ConnectionLoopAsync();
        }

        public void Dispose()
        {
            _udpClient.Close();
            _udpClient.Dispose();
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

        internal void LogInformation(string message) 
            => _logger?.LogInformation($"Матч (порт: {Port}): {message}");
        internal void LogError(string message) 
            => _logger?.LogError($"Матч (порт: {Port}): {message}");

        private async Task ConnectionLoopAsync()
        {
            LogInformation("Матч начал работу.");

            while (_timer.ElapsedMilliseconds < TimeForStarting)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                await ConnectionFrameAsync();
            }
        }

        private async Task ConnectionFrameAsync()
        {
            while (_udpClient.Available > 0)
            {
                var received = await _udpClient.ReceiveAsync();
                LogInformation($"Получено сообщение от {received.RemoteEndPoint}.");
                await State.ProcessMessageAsync(received.RemoteEndPoint, received.Buffer);
            }
        }
    }
}
