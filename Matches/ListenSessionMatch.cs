﻿using System;
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
    public class ListenSessionMatch : IMatch, IDisposable
    {
        private readonly ILogger<ListenSessionMatch> _logger;

        private UdpClient _udpClient;
        private CancellationToken _cancellationToken;
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
        {
            _logger = logger;
            ExpectedPlayers = playersEndPoints;
            TimeForStarting = timeForStarting;
            _udpClient = new UdpClient(port);
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
            LogInformation("Матч стартовал.");
            LogInformation("Ожидание игроков.");

            while (_timer.ElapsedMilliseconds < TimeForStarting)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                await ConnectionFrameAsync();
            }

            _udpClient.Close();
        }

        private async Task ConnectionFrameAsync()
        {
            if (_udpClient.Available > 0)
            {
                var received = await _udpClient.ReceiveAsync();
                LogInformation($"Получено сообщение от {received.RemoteEndPoint}.");
                await State.ProcessMessageAsync(received.RemoteEndPoint, received.Buffer);
            }
        }
    }
}
