using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Matchmaker.ViewModels
{
    public class UserViewModel
    {
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Логин слишком короткий")]
        public string Login { get; set; }
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Пароль слишком короткий")]
        public string Password { get; set; }
    }
}
