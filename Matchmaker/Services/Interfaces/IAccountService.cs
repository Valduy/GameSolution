using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matchmaker.ViewModels;
using Models;

namespace Matchmaker.Services
{
    public interface IAccountService
    {
        public Task RegisterAsync(UserViewModel model);
        public Task<User> AuthenticateAsync(UserViewModel model);
    }
}
