using System.Threading.Tasks;
using BCrypt.Net;
using Context;
using Matchmaker.Exceptions;
using Microsoft.EntityFrameworkCore;
using Models;
using BC = BCrypt.Net.BCrypt;

namespace Matchmaker.Services
{
    public class AccountService : IAccountService
    {
        private readonly GameDbContext _gameContext;

        public AccountService(GameDbContext gameContext)
        {
            _gameContext = gameContext;
        }

        public async Task RegisterAsync(User user)
        {
            try
            {
                user.Password = BC.HashPassword(user.Password);
                await _gameContext.Users.AddAsync(user);
                await _gameContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new AddItemException("Логин занят");
            }
        }

        public async Task<User> AuthenticateAsync(User user)
        {
            var result = await _gameContext.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
            return result != null 
                ? BC.Verify(user.Password, result.Password) ? result : null
                : null;
        }
    }
}
