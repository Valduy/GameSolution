using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matchmaker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class MatchmakerController : Controller
    {
        private IAuthorizationService _authorizationService;

        public MatchmakerController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpGet("queue")]
        public IActionResult Queue()
        {
            if (!_authorizationService.CheckAuthorization(HttpContext))
            {
                return Unauthorized();
            }

            return Ok();
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok();
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            if (!_authorizationService.CheckAuthorization(HttpContext))
            {
                return Unauthorized();
            }

            return Ok();
        }
    }
}
