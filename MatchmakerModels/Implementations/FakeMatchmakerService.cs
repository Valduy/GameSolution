using System;
using System.Collections.Generic;
using System.Text;
using MatchmakerServices.Interfaces;
using Network;

namespace MatchmakerServices.Implementations
{
    public class FakeMatchmakerService : IMatchmakerService
    {
        public bool Enqueue(string userId) => true;
        public UserStatus GetStatus(string userId) => UserStatus.Wait;
        public int? GetMatch(string userId) => 1;
        public bool Remove(string userId) => true;
    }
}
