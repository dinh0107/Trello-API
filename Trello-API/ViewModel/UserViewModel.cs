using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.ViewModel
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime? BirthDate { get; set; } 
    }
    public class UserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public DateTime? BirthDate { get; set; } 
        public string AvatarUrl { get; set; }
    }

    public class BoardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BackgroundImage { get; set; }
    }

}