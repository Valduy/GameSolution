using Matchmaker.Helpers;
using Matchmaker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network;
using Network.Messages;

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
        [HttpPost("enqueue")]
        public void Enqueue([FromBody]ClientEndPoint privateEndPoint)
        {
            var userId = User.GetName();
            var publicIp = HttpContext.Connection.RemoteIpAddress;
            var publicEndPoint = new ClientEndPoint(publicIp.ToString(), 0);
            var endPoints = new ClientEndPoints(publicEndPoint, privateEndPoint);
            _matchmakerService.Enqueue(userId, endPoints);
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
