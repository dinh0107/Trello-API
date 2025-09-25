using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.ViewModel
{
    public class LoginRequest
    {
        public string Username { get; set; }
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
        public string Username { get; set; }
        public string Password { get; set; }
    }
}