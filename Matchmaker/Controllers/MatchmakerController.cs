using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class MatchmakerController : Controller
    {
        public MatchmakerController()
        {

        }

        public async Task<IActionResult> Queue()
        {
            return Ok();
        }
    }
}
