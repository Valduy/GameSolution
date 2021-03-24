using System.ComponentModel.DataAnnotations;

namespace Matchmaker.ViewModels
{
    /// <summary>
    /// Модель представления для <see cref="Models.User"/>
    /// </summary>
    public class UserViewModel
    {
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Логин может иметь длину не менее {2} и не более {1} символов.")]
        [RegularExpression(@"^[a-zA-Z0-9_]*", ErrorMessage = "Неверный формат логина.")]
        public string Login { get; set; }
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Пароль может иметь длину не менее {2} и не более {1} символов.")]
        [RegularExpression(@"^[a-zA-Z0-9_]*", ErrorMessage = "Неверный формат пароля.")]
        public string Password { get; set; }
    }
}
