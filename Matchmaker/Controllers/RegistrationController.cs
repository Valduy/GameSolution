using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Context;
using Matchmaker.Exceptions;
using Matchmaker.Services.Interfaces;
using Matchmaker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class RegistrationController : Controller
    {
        private readonly IAccountService _accountService;

        public RegistrationController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _accountService.RegisterAsync(model);
            }
            catch(AddItemException ex)
            {
                throw new HttpStatusException(HttpStatusCode.Conflict, ex.Message);
            }

            return Ok();
        }
    }
}
