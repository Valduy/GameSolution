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

        private List<QueuedPlayer> _enqueued = new List<QueuedPlayer>();

        public MatchmakerModel()
        {

        }

        public bool Enqueue(string userId)
        {
            if (_enqueued.Any(o => o.Id == userId))
            {
                return false;
            }

            _enqueued.Add(new QueuedPlayer(userId));
            return true;
        }

        public UserStatus GetStatus(string userId)
        {
            throw new NotImplementedException();
        }

        public uint? GetMatch(string userId)
        {
            throw new NotImplementedException();
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
