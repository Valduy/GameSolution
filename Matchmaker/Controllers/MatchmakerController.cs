using Matchmaker.Helpers;
using Matchmaker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class MatchmakerController : Controller
    {
        private readonly IMatchmakerService _matchmakerService;

        public MatchmakerController(
            IMatchmakerService matchmakerService)
        {
            _matchmakerService = matchmakerService;
        }

        [Authorize]
        [HttpGet("enqueue")]
        public void Enqueue()
        {
            var userId = User.GetName();
            _matchmakerService.Enqueue(userId);
        }

        [Authorize]
        [HttpGet("status/get")]
        public UserStatus GetStatus()
        {
            var userId = User.GetName();
            return _matchmakerService.GetStatus(userId);
        }

        [Authorize]
        [HttpGet("match/get")]
        public int? GetMatch()
        {
            var userId = User.GetName();
            return _matchmakerService.GetMatch(userId);
        }

        [Authorize]
        [HttpGet("remove")]
        public bool Remove()
        {
            var userId = User.GetName();
            return _matchmakerService.Remove(userId);
        }
    }
}
