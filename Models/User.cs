using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        [JsonIgnore]
        public List<Friendship> FriendList { get; set; }
    }
}
