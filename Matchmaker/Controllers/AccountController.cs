using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Matchmaker.Exceptions;
using Matchmaker.Helpers;
using Matchmaker.Services;
using Matchmaker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;
using Network.Messages;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            IMapper mapper,
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Метод регистрирует пользователя в системе.
        /// </summary>
        /// <param name="model"><see cref="UserViewModel"/>.</param>
        /// <returns>
        /// <see cref="BadRequestObjectResult"/> с ошибками, возникшими при волидации, если <see cref="UserViewModel"/> не валидна.
        /// <see cref="OkResult"/>, если регистрация прошла успешно.
        /// </returns>
        /// <exception cref="HttpStatusException">Выбрасывается, когда логин уже занят.</exception>
        [HttpPost("registration")]
        public async Task<IActionResult> Register([FromBody]UserViewModel model)
        {
            _logger.LogInformation($"Пользователь пытается зарегестрироваться под логином: {model.Login}.");
            
            if (!ModelState.IsValid)
            {
                _logger.LogInformation($"Пользователь {model.Login} указал некорректный логи или пароль.");
                return BadRequest(ModelState);
            }

            try
            {
                var user = _mapper.Map<User>(model);
                await _accountService.RegisterAsync(user);
            }
            catch(AddItemException ex)
            {
                _logger.LogInformation($"Пользователь {model.Login} уже есть в базе.");
                throw new HttpStatusException(HttpStatusCode.Conflict, ex.Message);
            }

            _logger.LogInformation($"Пользователь {model.Login} был успешно зарегистрирован.");
            return Ok();
        }

        [Authorize]
        [HttpGet("authorization")]
        public IActionResult CheckAuthorization() => Ok();

        [HttpPost("authorization")]
        public async Task<IActionResult> Authorize([FromBody] UserViewModel model)
        {
            _logger.LogInformation($"Пользователь {model.Login} пытается авторизоваться.");
            var identity = await GetIdentity(model);
            
            if (identity == null)
            {
                _logger.LogInformation($"Пользователь {model.Login} не был найден.");
                return Unauthorized();
            }

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer:AuthOptions.ISSUER,
                audience:AuthOptions.AUDIENCE,
                notBefore:now,
                claims:identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new TokenMessage(encodedJwt, identity.Name);
            _logger.LogInformation($"Пользователь {model.Login} был успешно авторизован.");
            return Ok(response);
        }

        private async Task<ClaimsIdentity> GetIdentity(UserViewModel model)
        {
            ClaimsIdentity identity = null;
            var user = _mapper.Map<User>(model);
            user = await _accountService.AuthenticateAsync(user);

            if (user != null)
            {
                var claims = new List<Claim> {new Claim(ClaimsIdentity.DefaultNameClaimType, user.Id.ToString())};
                identity = new ClaimsIdentity(
                    claims, 
                    "Token", 
                    ClaimsIdentity.DefaultNameClaimType, 
                    ClaimsIdentity.DefaultRoleClaimType);
            }

            return identity;
        }
    }
}
