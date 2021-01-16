using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchmakerModels.Models.Interfaces;

namespace MatchmakerModels.Models.Implementations
{
    public class MatchmakerModel : IMatchmakerModel, IDisposable
    {
        private bool _disposed = false;
        private Task _matchmakerTask;

        private readonly List<string> _waitingPlayers = new List<string>();
        private Dictionary<string, int> _playerToMatch = new Dictionary<string, int>();

        public MatchmakerModel()
        {

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

            return UserStatus.Ignored;
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

        public void Remove(string userId)
        {
            lock (_waitingPlayers)
            {
                var id = _waitingPlayers.FirstOrDefault(o => o == userId);

                if (id != null)
                {
                    _waitingPlayers.Remove(id);
                }
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
            _disposed = true;
        }

        private async Task ManageMatchmakingAsync()
        {

        }
    }
}
