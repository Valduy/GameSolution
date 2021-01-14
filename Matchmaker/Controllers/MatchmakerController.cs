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
        public async Task<IActionResult> Queue()
        {
            if (await _authorizationService.CheckAuthorizationAsync(HttpContext))
            {
                return Ok();
            }
            
            return Unauthorized();
        }
    }
}
