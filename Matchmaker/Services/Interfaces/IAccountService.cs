using System.Threading.Tasks;
using Models;

namespace Matchmaker.Services
{
    public interface IAccountService
    {
        public Task RegisterAsync(User user);
        public Task<User> AuthenticateAsync(User user);
    }
}
