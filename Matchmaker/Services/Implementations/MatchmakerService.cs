using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Matches;
using Matchmaker.Factories;
using Microsoft.Extensions.Logging;
using Network;
using Network.Messages;

namespace Matchmaker.Services
{
    // TODO: чистка давно не дававших о себе знать пользователей
    public class MatchmakerService : IMatchmakerService, IDisposable
    {
        private const int MatchCreationDelay = 1000 / 60;

        private readonly CancellationTokenSource _tokenSource;
        private readonly Dictionary<string, ClientEndPoints> _playersEndPoints = new Dictionary<string, ClientEndPoints>();
        private readonly Dictionary<string, int> _playerToMatch = new Dictionary<string, int>();

        private readonly IMatchFactory _matchFactory;
        private readonly ILogger<MatchmakerService> _logger;

        private bool _disposed;

        public int PlayersPerMatch => 2;

        public MatchmakerService(
            IMatchFactory matchFactory, 
            ILogger<MatchmakerService> logger)
        {
            _matchFactory = matchFactory;
            _logger = logger;
            _tokenSource = new CancellationTokenSource();
            Task.Run(() => MatchmakingLoop(_tokenSource.Token), _tokenSource.Token);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MatchmakerService()
        {
            Dispose(false);
        }

        public bool Enqueue(string userId, ClientEndPoints endPoints)
        {
            _logger.LogInformation($"Постановка в очередь (id пользователя: {userId}).");

            lock (_playersEndPoints)
            {
                if (_playersEndPoints.ContainsKey(userId))
                {
                    _logger.LogInformation($"Пользователь уже в очереди (id пользователя: {userId}).");
                    return false;
                }

                _playersEndPoints[userId] = endPoints;
                _logger.LogInformation($"Пользователь добавлен в очередь (id пользователя: {userId}).");
                return true;
            }
        }

        public UserStatus GetStatus(string userId)
        {
            _logger.LogInformation($"Запрос статуса (id пользователя: {userId}).");

            lock (_playersEndPoints)
            {
                if (_playersEndPoints.ContainsKey(userId))
                {
                    _logger.LogInformation($"Пользователь ожидает (id пользователя: {userId}).");
                    return UserStatus.Wait;
                }
            }

            lock (_playerToMatch)
            {
                if (_playerToMatch.ContainsKey(userId))
                {
                    _logger.LogInformation($"Пользователь добавлен в матч (id пользователя: {userId}).");
                    return UserStatus.Connected;
                }
            }

            _logger.LogInformation($"Пользователь отсутствует (id пользователя: {userId}).");
            return UserStatus.Absent;
        }

        public int? GetMatch(string userId)
        {
            _logger.LogInformation($"Запрос матча (id пользователя: {userId}).");

            lock (_playerToMatch)
            {
                if (_playerToMatch.TryGetValue(userId, out var port))
                {
                    _logger.LogInformation($"Матч найден (id пользователя: {userId}).");
                    return port;
                }

                _logger.LogInformation($"Матч не найден (id пользователя: {userId}).");
                return null;
            }
        }

        public bool Remove(string userId)
        {
            _logger.LogInformation($"Выход из очереди (id пользователя: {userId}).");

            lock (_playersEndPoints)
            {
                return _playersEndPoints.Remove(userId);
            }
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing) { }

            _tokenSource.Cancel();
            _disposed = true;
        }

        private async Task MatchmakingLoop(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                MatchmakingFrame(token);
                await Task.Delay(MatchCreationDelay, token);
            }
        }

        private void MatchmakingFrame(CancellationToken token)
        {
            lock (_playersEndPoints)
            {
                if (_playersEndPoints.Count < PlayersPerMatch) return;

                _logger.LogInformation("Создаем матч.");
                var playersPairs = _playersEndPoints.Take(PlayersPerMatch).ToList();
                var playersEndPoints = playersPairs.Select(o => o.Value).ToList();
                var matchPort = StartNewMatch(playersEndPoints);

                lock (_playerToMatch)
                {
                    playersPairs.ForEach(o => _playerToMatch[o.Key] = matchPort);
                }

                playersPairs.ForEach(o => _playersEndPoints.Remove(o.Key));
            }
        }

        private int StartNewMatch(IReadOnlyCollection<ClientEndPoints> playersEndPoints)
        {
            var match = _matchFactory.CreateMatch(playersEndPoints);
            match.MatchStarted += OnMatchStarted;
            Task.Run(async () => await match.WorkAsync(_tokenSource.Token), _tokenSource.Token);
            _logger.LogInformation($"Матч запущен на порту {match.Port}.");
            return match.Port;
        }

        private void OnMatchStarted(IMatch match)
        {
            _logger.LogInformation($"Матч стартовал (порт: {match.Port}).");

            lock (_playerToMatch)
            {
                var matchPlayers = _playerToMatch
                    .Where(o => o.Value == match.Port)
                    .Select(o => o.Key);

                foreach (var player in matchPlayers)
                {
                    _playerToMatch.Remove(player);
                }
            }
        }
    }
}
