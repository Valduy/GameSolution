using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matchmaker.Services;
using MatchmakerModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class MatchmakerController : Controller
    {
        private readonly ISimpleAuthorizationService _authorizationService;
        private readonly IMatchmakerModel _matchmakerModel;

        public MatchmakerController(
            ISimpleAuthorizationService authorizationService,
            IMatchmakerModel matchmakerModel)
        {
            _authorizationService = authorizationService;
            _matchmakerModel = matchmakerModel;
        }

        [HttpGet("enqueue")]
        public IActionResult Enqueue() 
            => OkOnAuthorized(() => _matchmakerModel.Enqueue(_authorizationService.GetIdentifier(HttpContext)));

        [HttpGet("status/get")]
        public IActionResult GetStatus() 
            => OkOnAuthorized(() => _matchmakerModel.GetStatus(_authorizationService.GetIdentifier(HttpContext)));

        [HttpGet("match/get")]
        public IActionResult GetMatch() 
            => OkOnAuthorized(() => _matchmakerModel.GetMatch(_authorizationService.GetIdentifier(HttpContext)));

        [HttpGet("remove")]
        public IActionResult Remove() 
            => OkOnAuthorized(() => _matchmakerModel.Remove(_authorizationService.GetIdentifier(HttpContext)));

        private IActionResult OkOnAuthorized(Func<object> func) 
            => _authorizationService.CheckAuthorization(HttpContext) 
                ? (IActionResult)Ok(func?.Invoke()) 
                : Unauthorized();
    }
}
