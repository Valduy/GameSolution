using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matchmaker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class AuthorizationController : Controller
    {
        private IAuthorizationService _authorizationService;

        public AuthorizationController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("authorize")]
        public void Authorize()
        {
            _authorizationService.Authorize(HttpContext);
        }
    }
}
