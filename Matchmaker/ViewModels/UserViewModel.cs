using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Matchmaker.ViewModels
{
    public class UserViewModel
    {
        [StringLength(int.MaxValue, MinimumLength = 4, ErrorMessage = "Short login")]
        public string Login { get; set; }
        [StringLength(int.MaxValue, MinimumLength = 8, ErrorMessage = "Short password")]
        public string Password { get; set; }
    }
}
