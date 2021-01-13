using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationService.Controllers
{
    [Route("api/[controller]")]
    public class AuthorizationController : Controller
    {
        [HttpPost("authorize")]
        public IActionResult Authorize()
        {
            HttpContext.Session.SetString("role", "player");
            return Ok(HttpContext.Session.Id);
        }

        [HttpGet("id")]
        public IActionResult GetId()
        {
            if (HttpContext.Session.Keys.Contains("role"))
            {
                return Ok(HttpContext.Session.Id);
            }

            return Unauthorized();
        }
    }
}
