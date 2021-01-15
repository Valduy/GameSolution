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

        private readonly Queue<QueuedPlayer> _waitingPlayers = new Queue<QueuedPlayer>();
        private Dictionary<string, int> _playerToMatch = new Dictionary<string, int>();

        public MatchmakerModel()
        {

        }

        public bool Enqueue(string userId)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Any(o => o.Id == userId))
                {
                    return false;
                }

                _waitingPlayers.Enqueue(new QueuedPlayer(userId));
                return true;
            }
        }

        public UserStatus GetStatus(string userId)
        {
            lock (_waitingPlayers)
            {
                if (_waitingPlayers.Any(o => o.Id == userId))
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

        ~MatchmakerModel()
        {
            Dispose(false);
        }
    }
}
