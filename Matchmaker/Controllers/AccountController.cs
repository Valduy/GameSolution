using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Context;
using Matchmaker.Exceptions;
using Matchmaker.Helpers;
using Matchmaker.Services;
using Matchmaker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
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

        [HttpPost("authorize")]
        public async Task<IActionResult> Authorize([FromBody] UserViewModel model)
        {
            var identity = await GetIdentity(model);
            if (identity == null) return Unauthorized();

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer:AuthOptions.ISSUER,
                audience:AuthOptions.AUDIENCE,
                notBefore:now,
                claims:identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new
            {
                access_token = encodedJwt,
                id = identity.Name,
            };

            return Ok(response);
        }

        private async Task<ClaimsIdentity> GetIdentity(UserViewModel model)
        {
            ClaimsIdentity identity = null;
            var user = await _accountService.AuthenticateAsync(model);

            if (user != null)
            {
                var claims = new List<Claim> {new Claim(ClaimsIdentity.DefaultNameClaimType, user.Id.ToString())};
                identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            }

            return identity;
        }
    }
}
