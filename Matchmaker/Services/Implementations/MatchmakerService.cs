using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GameLoops;
using Matches;
using Matchmaker.Factories;
using Network;
using Network.Messages;

namespace Matchmaker.Services
{
    public class MatchmakerService : IMatchmakerService, IDisposable
    {
        private readonly FixedFpsGameLoop _matchmakingLoop;
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _cancellationToken;

        private readonly List<Task> _matchesTasks = new List<Task>();
        private readonly Dictionary<string, ClientEndPoints> _playersEndPoints = new Dictionary<string, ClientEndPoints>();
        private readonly Dictionary<string, int> _playerToMatch = new Dictionary<string, int>();

        private readonly IMatchFactory _matchFactory;

        private bool _disposed;

        public int PlayersPerMatch => 2;

        public MatchmakerService(IMatchFactory matchFactory)
        {
            _matchFactory = matchFactory;
            _matchmakingLoop = new FixedFpsGameLoop(ManageMatchmaking, 60);
            _tokenSource = new CancellationTokenSource();
            _cancellationToken = _tokenSource.Token;
            _matchmakingLoop.Start();
        }

        ~MatchmakerService()
        {
            Dispose(false);
        }

        public bool Enqueue(string userId, ClientEndPoints endPoints)
        {
            lock (_playersEndPoints)
            {
                if (_playersEndPoints.ContainsKey(userId))
                {
                    return false;
                }

                _playersEndPoints[userId] = endPoints;
                return true;
            }
        }

        public UserStatus GetStatus(string userId)
        {
            lock (_playersEndPoints)
            {
                if (_playersEndPoints.ContainsKey(userId))
                {
                    return UserStatus.Wait;
                }
            }

            lock (_playerToMatch)
            {
                if (_playerToMatch.ContainsKey(userId))
                {
                    return UserStatus.Connected;
                }
            }

            return UserStatus.Absent;
        }

        public int? GetMatch(string userId)
        {
            lock (_playerToMatch)
            {
                if (_playerToMatch.TryGetValue(userId, out var port))
                {
                    return port;
                }

                return null;
            }
        }

        public bool Remove(string userId)
        {
            lock (_playersEndPoints)
            {
                return _playersEndPoints.Remove(userId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing) { }

            _matchmakingLoop.Stop();
            _tokenSource.Cancel();

            lock (_matchesTasks)
            {
                Task.WaitAll(_matchesTasks.ToArray());
            }

            _disposed = true;
        }

        private void ManageMatchmaking(double dt)
        {
            lock (_playersEndPoints)
            {
                if (_playersEndPoints.Count < PlayersPerMatch) return;

                try
                {
                    var playersPairs = _playersEndPoints
                        .Take(PlayersPerMatch)
                        .ToList();

                    var playersEndPoints = playersPairs
                        .Select(o => o.Value)
                        .ToList();

                    var matchPort = StartNewMatch(playersEndPoints);

                    lock (_playerToMatch)
                    {
                        playersPairs.ForEach(o => _playerToMatch[o.Key] = matchPort);
                    }

                    playersPairs.ForEach(o => _playersEndPoints.Remove(o.Key));
                }
                catch (SocketException ex)
                {
                    // TODO: log
                }
            }
        }

        private int StartNewMatch(IReadOnlyCollection<ClientEndPoints> playersEndPoints)
        {
            var match = _matchFactory.CreateMatch(playersEndPoints);
            match.MatchStarted += OnMatchStarted;
            
            lock (_matchesTasks)
            {
                var matchTask = new Task(async () => await match.WorkAsync(_cancellationToken), _cancellationToken)
                    .ContinueWith(OnMatchTaskEnded, _cancellationToken);
                _matchesTasks.Add(matchTask);
                matchTask.Start();
            }

            return match.Port;
        }

        private void OnMatchStarted(IMatch match)
        {
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

        private void OnMatchTaskEnded(Task task)
        {
            lock (_matchesTasks)
            {
                _matchesTasks.Remove(task);
            }
        }
    }
}
