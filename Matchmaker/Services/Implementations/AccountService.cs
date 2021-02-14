using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Context;
using Matchmaker.Exceptions;
using Matchmaker.ViewModels;
using Microsoft.EntityFrameworkCore;
using Models;
using BC = BCrypt.Net.BCrypt;

namespace Matchmaker.Services
{
    public class AccountService : IAccountService
    {
        private readonly GameDbContext _gameContext;
        private readonly IMapper _mapper;

        public AccountService(
            GameDbContext gameContext,
            IMapper mapper)
        {
            _gameContext = gameContext;
            _mapper = mapper;
        }

        public async Task RegisterAsync(UserViewModel model)
        {
            try
            {
                var user = _mapper.Map<User>(model);
                user.Password = BC.HashPassword(user.Password);
                await _gameContext.Users.AddAsync(user);
                await _gameContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new AddItemException("Логин занят");
            }
        }

        public async Task<User> AuthenticateAsync(UserViewModel model) 
            => await _gameContext.Users.FirstOrDefaultAsync(u => u.Login == model.Login);
    }
}
