using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Matches;
using Matchmaker.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Network;
using Network.Messages;

namespace Matchmaker.Services
{
    public class MatchmakerService : IMatchmakerService, IDisposable
    {
        private class ClientRecord
        {
            public ClientEndPoints EndPoints { get; }
            public long LastRequestTime { get; set; }

            public ClientRecord(ClientEndPoints endPoints, long lastRequestTime)
            {
                EndPoints = endPoints;
                LastRequestTime = lastRequestTime;
            }
        }

        private const int MatchmakerDelay = 1000 / 60;
        private const int CleaningDelay = 5000;
        private const int AllowedDelta = CleaningDelay;

        private readonly CancellationTokenSource _tokenSource;
        private readonly Dictionary<string, ClientRecord> _playersRecords = new Dictionary<string, ClientRecord>();
        private readonly Dictionary<string, IMatch> _playerToMatch = new Dictionary<string, IMatch>();

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

            Task.Run(
                () => DelayedLoop(MatchmakingFrame, MatchmakerDelay, _tokenSource.Token), 
                _tokenSource.Token);

            Task.Run(
                () => DelayedLoop(CleaningFrame, CleaningDelay, _tokenSource.Token),
                _tokenSource.Token);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Enqueue(string userId, ClientEndPoints endPoints)
        {
            _logger.LogInformation($"Постановка в очередь (id пользователя: {userId}).");

            lock (_playersRecords)
            {
                if (_playersRecords.ContainsKey(userId))
                {
                    _logger.LogInformation($"Пользователь уже в очереди (id пользователя: {userId}).");
                    return false;
                }

                _playersRecords[userId] = new ClientRecord(endPoints, GetCurrentTime());
                _logger.LogInformation($"Пользователь добавлен в очередь (id пользователя: {userId}).");
                return true;
            }
        }

        public UserStatus GetStatus(string userId)
        {
            _logger.LogInformation($"Запрос статуса (id пользователя: {userId}).");

            lock (_playersRecords)
            {
                if (_playersRecords.TryGetValue(userId, out var record))
                {
                    _logger.LogInformation($"Пользователь ожидает (id пользователя: {userId}).");
                    record.LastRequestTime = GetCurrentTime();
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
                if (_playerToMatch.TryGetValue(userId, out var match))
                {
                    _logger.LogInformation($"Матч найден (id пользователя: {userId}).");
                    return match.Port;
                }

                _logger.LogInformation($"Матч не найден (id пользователя: {userId}).");
                return null;
            }
        }

        public bool Remove(string userId)
        {
            _logger.LogInformation($"Выход из очереди (id пользователя: {userId}).");

            lock (_playersRecords)
            {
                return _playersRecords.Remove(userId);
            }
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _disposed = true;
            }
        }

        private async Task DelayedLoop(Action action, int delay, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                action();
                await Task.Delay(delay, cancellationToken);
            }
        }

        private void MatchmakingFrame()
        {
            lock (_playersRecords)
            {
                while (_playersRecords.Count >= PlayersPerMatch)
                {
                    _logger.LogInformation("Создаем матч.");
                    var playersPairs = _playersRecords.Take(PlayersPerMatch).ToList();
                    var playersEndPoints = playersPairs.Select(o => o.Value.EndPoints).ToList();
                    var match = StartNewMatch(playersEndPoints);

                    lock (_playerToMatch)
                    {
                        playersPairs.ForEach(o => _playerToMatch[o.Key] = match);
                    }

                    playersPairs.ForEach(o => _playersRecords.Remove(o.Key));
                }
            }
        }

        private IMatch StartNewMatch(IReadOnlyCollection<ClientEndPoints> playersEndPoints)
        {
            var match = _matchFactory.CreateMatch(playersEndPoints);
            match.MatchStarted += RemoveAssociatedPlayers;

            Task.Run(async () => await match.WorkAsync(_tokenSource.Token), _tokenSource.Token)
                .ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        RemoveAssociatedPlayers(match);
                    }

                    match.Dispose();
                });

            _logger.LogInformation($"Матч запущен на порту {match.Port}.");
            return match;
        }

        private void RemoveAssociatedPlayers(IMatch match)
        {
            lock (_playerToMatch)
            {
                 _playerToMatch
                     .Where(pair => pair.Value == match)
                     .ToList()
                     .ForEach(pair => _playerToMatch.Remove(pair.Key));
            }
        }

        private void CleaningFrame()
        {
            lock (_playersRecords)
            {
                long currentTime = GetCurrentTime();
                _playersRecords
                    .Where(record => currentTime - record.Value.LastRequestTime > AllowedDelta)
                    .ToList()
                    .ForEach(pair => _playersRecords.Remove(pair.Key));
            }
        }

        private long GetCurrentTime() 
            => ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
    }
}
