using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DBRepository;
using Matchmaker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class RegistrationController : Controller
    {
        private readonly GameDbContext _gameContext;
        private readonly IMapper _mapper;

        public RegistrationController(
            GameDbContext gameContext,
            IMapper mapper)
        {
            _gameContext = gameContext;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<User>(userViewModel);
                await _gameContext.Users.AddAsync(user);
                await _gameContext.SaveChangesAsync();
                return Ok();
            }

            return BadRequest(ModelState);
        }
    }
}
