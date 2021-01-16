using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matchmaker.Services.Interfaces;
using MatchmakerModels.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class MatchmakerController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IMatchmakerModel _matchmakerModel;

        public MatchmakerController(
            IAuthorizationService authorizationService,
            IMatchmakerModel matchmakerModel)
        {
            _authorizationService = authorizationService;
            _matchmakerModel = matchmakerModel;
        }

        [HttpGet("enqueue")]
        public IActionResult Enqueue()
        {
            return OkOnAuthorized(() => _matchmakerModel.Enqueue(_authorizationService.GetIdentifier(HttpContext)));
        }

        [HttpGet("status/get")]
        public IActionResult GetStatus()
        {
            return OkOnAuthorized(() => _matchmakerModel.GetStatus(_authorizationService.GetIdentifier(HttpContext)));
        }

        [HttpGet("match/get")]
        public IActionResult GetMatch()
        {
            return OkOnAuthorized(() => _matchmakerModel.GetMatch(_authorizationService.GetIdentifier(HttpContext)));
        }

        [HttpGet("remove")]
        public IActionResult Remove()
        {
            return OkOnAuthorized(() => _matchmakerModel.Remove(_authorizationService.GetIdentifier(HttpContext)));
        }

        private IActionResult OkOnAuthorized(Func<object> func)
        {
            return _authorizationService.CheckAuthorization(HttpContext) 
                ? (IActionResult)Ok(func?.Invoke()) 
                : Unauthorized();
        }
    }
}
