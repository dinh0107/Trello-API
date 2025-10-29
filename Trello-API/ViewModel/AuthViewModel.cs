using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Trello_API.ViewModel
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class UpdateUserInfoRequest
    {
        [Required]
        public string FullName { get; set; }

        public string Phone { get; set; }

        public DateTime? BirthDate { get; set; }

        public HttpPostedFileBase Avatar { get; set; }
    }

}