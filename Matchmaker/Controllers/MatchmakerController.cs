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

        [HttpGet("Enqueue")]
        public IActionResult Enqueue()
        {
            if (!_authorizationService.CheckAuthorization(HttpContext))
            {
                return Unauthorized();
            }

            var identifier = _authorizationService.GetIdentifier(HttpContext);
            // TODO: постановка клиента в очередь
            return Ok();
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            if (!_authorizationService.CheckAuthorization(HttpContext))
            {
                return Unauthorized();
            }

            return Ok();
        }
    }
}
