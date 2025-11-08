using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using Trello_API.DAL;
using Trello_API.Helper;
using Trello_API.Models;
using Trello_API.ViewModel;

namespace Trello_API.Controllers
{
    [Authorize]
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        [AllowAnonymous]
        [HttpPost, Route("login")]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return Content(HttpStatusCode.BadRequest, new { Success = false, Message = "Vui lòng nhập đầy đủ Email và Mật khẩu" });

            var user = _unitOfWork.UserRepository.GetQuery(u => u.Email == request.Email).FirstOrDefault();
            if (user == null)
                return Content(HttpStatusCode.NotFound, new { Success = false, Message = "Tài khoản không tồn tại" });

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Content(HttpStatusCode.Unauthorized, new { Success = false, Message = "Mật khẩu không chính xác" });

            var accessToken = JwtHelper.GenerateJwtToken(user.Email, 120); // 2 giờ
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _unitOfWork.Save();

            var cookie = new HttpCookie("AccessToken", accessToken)
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(120)
            };
            HttpContext.Current.Response.Cookies.Add(cookie);

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập thành công",
                User = new { user.Id, user.Email }
            });
        }


        [AllowAnonymous]
        [HttpPost, Route("refresh")]
        public IHttpActionResult Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return Content(HttpStatusCode.BadRequest, new { Success = false, Message = "Refresh token không được để trống" });

            var user = _unitOfWork.UserRepository.GetQuery(u => u.RefreshToken == request.RefreshToken).FirstOrDefault();
            if (user == null)
                return Content(HttpStatusCode.NotFound, new { Success = false, Message = "Refresh token không hợp lệ" });

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                return Content(HttpStatusCode.Unauthorized, new { Success = false, Message = "Refresh token đã hết hạn" });

            var newAccessToken = JwtHelper.GenerateJwtToken(user.Email, 120);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _unitOfWork.Save();

            var cookie = new HttpCookie("AccessToken", newAccessToken)
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(120)
            };
            HttpContext.Current.Response.Cookies.Add(cookie);

            return Ok(new
            {
                Success = true,
                Message = "Cấp mới AccessToken thành công",
                RefreshToken = newRefreshToken
            });
        }



        [AllowAnonymous]
        [HttpPost, Route("refresh")]
        public IHttpActionResult Refresh()
        {
            var refreshTokenCookie = HttpContext.Current.Request.Cookies["RefreshToken"];
            if (refreshTokenCookie == null)
                return Content(HttpStatusCode.BadRequest, new { Success = false, Message = "Không có refresh token" });

            var refreshToken = refreshTokenCookie.Value;
            var user = _unitOfWork.UserRepository.GetQuery(u => u.RefreshToken == refreshToken).FirstOrDefault();
            if (user == null)
                return Content(HttpStatusCode.NotFound, new { Success = false, Message = "Refresh token không hợp lệ" });

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                return Content(HttpStatusCode.Unauthorized, new { Success = false, Message = "Refresh token đã hết hạn" });
            var newAccessToken = JwtHelper.GenerateJwtToken(user.Email, 120);
            var newRefreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _unitOfWork.Save();

            var cookie = new HttpCookie("AccessToken", newAccessToken)
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(120)
            };
            HttpContext.Current.Response.Cookies.Add(cookie);

            var refreshCookie = new HttpCookie("RefreshToken", newRefreshToken)
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            HttpContext.Current.Response.Cookies.Add(refreshCookie);

            return Ok(new { Success = true, Message = "Cấp mới AccessToken thành công" });
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("check-auth")]
        public IHttpActionResult CheckAuth()
        {
            var tokenCookie = HttpContext.Current.Request.Cookies["AccessToken"];
            if (tokenCookie == null)
                return Ok(new { isAuthenticated = false });

            var principal = JwtHelper.ValidateToken(tokenCookie.Value);
            return Ok(new { isAuthenticated = principal != null });
        }

        [Authorize]
        [HttpPost]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            var email = User.Identity.Name;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == email)
                .FirstOrDefault();

            if (user == null)
                return NotFound();

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            _unitOfWork.Save();

            var cookie = new HttpCookie("AccessToken", "")
            {
                HttpOnly = true,

                Secure = HttpContext.Current.Request.IsSecureConnection,
                SameSite = HttpContext.Current.Request.IsSecureConnection
                    ? SameSiteMode.None 
                    : SameSiteMode.Lax, 

                Expires = DateTime.UtcNow.AddDays(-1)
            };
            var refreshCookie = new HttpCookie("RefreshToken", "")
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            HttpContext.Current.Response.Cookies.Add(refreshCookie);

            HttpContext.Current.Response.Cookies.Add(cookie);

            return Ok(new
            {
                Success = true,
                Message = "Đăng xuất thành công"
            });
        }
        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }

    }
}