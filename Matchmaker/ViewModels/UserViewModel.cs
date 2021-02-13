using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Matchmaker.ViewModels
{
    public class UserViewModel
    {
        [RegularExpression(@"^[a-zA-Z0-9_]{4,20}", ErrorMessage = "Неверный формат логина")]
        public string Login { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]{4,20}", ErrorMessage = "Неверный формат пароля")]
        public string Password { get; set; }
    }
}
