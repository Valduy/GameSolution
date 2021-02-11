using System;
using System.Collections.Generic;

namespace Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public List<Friendship> FriendList { get; set; }
    }
}
