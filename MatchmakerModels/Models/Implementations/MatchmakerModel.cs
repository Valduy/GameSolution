using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matches;
using MatchmakerModels.Models.Interfaces;

namespace MatchmakerModels.Models.Implementations
{
    public class MatchmakerModel : IMatchmakerModel, IDisposable
    {
        private bool _disposed = false;
        private Task _matchmakerTask;

        private readonly List<string> _waitingPlayers = new List<string>();
        private readonly List<IMatch> _matches = new List<IMatch>();
        private readonly Dictionary<string, int> _playerToMatch = new Dictionary<string, int>();

        public MatchmakerModel()
        {
            _matchmakerTask = Task.Run(ManageMatchmaking);
        }

        ~MatchmakerModel()
        {
            Dispose(false);
        }

        public bool Enqueue(string userId)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Any(o => o == userId))
                {
                    return false;
                }

                _waitingPlayers.Add(userId);
                return true;
            }
        }

        public UserStatus GetStatus(string userId)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Any(o => o == userId))
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
                    _playerToMatch.Remove(userId);
                    return port;
                }

                return null;
            }
        }

        public bool Remove(string userId)
        {
            lock (_waitingPlayers)
            {
                var id = _waitingPlayers.FirstOrDefault(o => o == userId);
                if (id == null) return false;
                _waitingPlayers.Remove(id);
                return true;
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

            // TODO: прервать таск матчмейкинга
            // TODO: прервать матчи
            _disposed = true;
        }

        private void ManageMatchmaking()
        {
            // TODO: использовать игровой цикл
            while (true)
            {
                TryCreateMatch();
            }
        }

        private bool TryCreateMatch()
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Count >= ListenSession.PlayersCount)
                {
                    var match = new ListenSession();
                    match.Started += OnMatchStarted;
                    match.Ended += OnMatchEnded;

                    for (int i = 0; i < ListenSession.PlayersCount; i++)
                    {
                        var player = _waitingPlayers.Last();
                        _playerToMatch[player] = match.Port;
                        _waitingPlayers.Remove(player);
                    }

                    return true;
                }

                return false;
            }
        }

        private void OnMatchStarted(IMatch match)
        {
            lock (_playerToMatch)
            {
                var latePlayers = _playerToMatch
                    .Where(o => o.Value == match.Port)
                    .Select(o => o.Key);

                foreach (var player in latePlayers)
                {
                    _playerToMatch.Remove(player);
                }
            }
        }

        private void OnMatchEnded(IMatch match)
        {
            _matches.Remove(match);
        }
    }
}
