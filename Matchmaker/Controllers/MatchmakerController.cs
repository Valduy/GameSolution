using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matchmaker.Services;
using MatchmakerServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class MatchmakerController : Controller
    {
        private readonly ISimpleAuthorizationService _authorizationService;
        private readonly IMatchmakerService _matchmakerService;

        public MatchmakerController(
            ISimpleAuthorizationService authorizationService,
            IMatchmakerService matchmakerService)
        {
            _authorizationService = authorizationService;
            _matchmakerService = matchmakerService;
        }

        [HttpGet("enqueue")]
        public IActionResult Enqueue() 
            => OkOnAuthorized(() => _matchmakerService.Enqueue(_authorizationService.GetIdentifier(HttpContext)));

        [HttpGet("status/get")]
        public IActionResult GetStatus() 
            => OkOnAuthorized(() => _matchmakerService.GetStatus(_authorizationService.GetIdentifier(HttpContext)));

        [HttpGet("match/get")]
        public IActionResult GetMatch() 
            => OkOnAuthorized(() => _matchmakerService.GetMatch(_authorizationService.GetIdentifier(HttpContext)));

        [HttpGet("remove")]
        public IActionResult Remove() 
            => OkOnAuthorized(() => _matchmakerService.Remove(_authorizationService.GetIdentifier(HttpContext)));

        private IActionResult OkOnAuthorized(Func<object> func) 
            => _authorizationService.CheckAuthorization(HttpContext) 
                ? (IActionResult)Ok(func?.Invoke()) 
                : Unauthorized();
    }
}
