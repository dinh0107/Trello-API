using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string RefreshToken { get; set; }

        public DateTime? BirthDate { get; set; }
        public DateTime Creatdate { get; set; } = DateTime.UtcNow;
        public DateTime RefreshTokenExpiry { get; set; } = DateTime.UtcNow;
        public ICollection<Board> Boards { get; set; }
        public virtual ICollection<BoardUser> BoardUsers { get; set; }
    }
}