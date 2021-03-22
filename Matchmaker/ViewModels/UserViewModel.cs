using System.ComponentModel.DataAnnotations;

namespace Matchmaker.ViewModels
{
    public class UserViewModel
    {
        [RegularExpression(@"^[a-zA-Z0-9_]{4,20}", ErrorMessage = "Неверный формат логина")]
        public string Login { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]{8,20}", ErrorMessage = "Неверный формат пароля")]
        public string Password { get; set; }
    }
}
