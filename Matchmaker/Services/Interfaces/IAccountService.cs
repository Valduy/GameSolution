using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matchmaker.ViewModels;

namespace Matchmaker.Services.Interfaces
{
    public interface IAccountService
    {
        public Task RegisterAsync(UserViewModel model);
        public Task<bool> AuthenticateAsync(UserViewModel model);
    }
}
