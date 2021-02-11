using System;
using System.Collections.Generic;
using System.Linq;
using GameLoops;
using Matches;
using MatchmakerServices.Interfaces;

namespace MatchmakerServices.Implementations
{
    public class MatchmakerService<TMatch> : IMatchmakerService, IDisposable where TMatch : MatchBase, new()
    {
        private readonly FixedFpsGameLoop _matchmakingLoop;
        
        private readonly List<string> _waitingPlayers = new List<string>();
        private readonly List<MatchBase> _matches = new List<MatchBase>();
        private readonly Dictionary<string, int> _playerToMatch = new Dictionary<string, int>();

        private bool _disposed;

        public int PlayersPerMatch { get; }

        public MatchmakerService(int playersPerMatch)
        {
            PlayersPerMatch = playersPerMatch;
            _matchmakingLoop = new FixedFpsGameLoop(ManageMatchmaking, 60);
            _matchmakingLoop.Start();
        }

        ~MatchmakerService()
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

            _matchmakingLoop.Stop();
            _disposed = true;
        }

        private void ManageMatchmaking(double dt)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Count >= PlayersPerMatch)
                {
                    var match = new TMatch();

                    for (int i = 0; i < PlayersPerMatch; i++)
                    {
                        var player = _waitingPlayers.Last();
                        _playerToMatch[player] = match.Port;
                        _waitingPlayers.Remove(player);
                    }
                }
            }
        }

        private void OnMatchStarted(MatchBase matchBase)
        {
            lock (_playerToMatch)
            {
                var matchPlayers = _playerToMatch
                    .Where(o => o.Value == matchBase.Port)
                    .Select(o => o.Key);

                foreach (var player in matchPlayers)
                {
                    _playerToMatch.Remove(player);
                }
            }
        }

        private void OnMatchEnded(MatchBase matchBase)
        {
            _matches.Remove(matchBase);
        }
    }
}
