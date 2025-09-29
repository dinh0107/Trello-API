using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Trello_API.DAL;
using Trello_API.ViewModel;

namespace Trello_API.Controllers
{
    [Authorize]
    [RoutePrefix("api/account")]
    public class UserController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();


        [HttpGet]
        [Route("me")]
        public IHttpActionResult GetCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userName = identity.Name; 
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var fullName = identity.FindFirst("fullName")?.Value;
            var phone = identity.FindFirst("phone")?.Value;
            var avatarUrl = identity.FindFirst("avatarUrl")?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = userName,
                FullName = fullName,
                Phone = phone,
                AvatarUrl = avatarUrl
            });
        }
        [HttpPut]
        [Route("update-info")]
        public IHttpActionResult UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            if (request == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var identity = (ClaimsIdentity)User.Identity;
            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var user = _unitOfWork.UserRepository.GetQuery(u => u.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrEmpty(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;

            if (request.BirthDate.HasValue)
                user.Creatdate = request.BirthDate.Value;

            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Cập nhật thông tin thành công",
                User = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.AvatarUrl,
                    BirthDate = user.Creatdate 
                }
            });
        }
    }
}
