using Network;
using Network.Messages;

namespace Matchmaker.Services
{
    public class FakeMatchmakerService : IMatchmakerService
    {
        public bool Enqueue(string userId, ClientEndPoints endPoints) => true;
        public UserStatus GetStatus(string userId) => UserStatus.Wait;
        public int? GetMatch(string userId) => 1;
        public bool Remove(string userId) => true;
    }
}
