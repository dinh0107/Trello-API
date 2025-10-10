using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
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
        [Authorize]
        [Route("me")]
        public IHttpActionResult GetCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var user  = _unitOfWork.UserRepository.GetQuery(u => u.Email == identity.Name).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            var UserInfo = new UserDto
            {
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                BirthDate = user.BirthDate,
                AvatarUrl = user.AvatarUrl,
            };
            return Ok(UserInfo);
        }
        [HttpPut]
        [Route("update-info")]
        public async Task<IHttpActionResult> UpdateCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository.GetQuery(u => u.Email == identity.Name).FirstOrDefault();
            if (user == null) return NotFound();

            var provider = await Request.Content.ReadAsMultipartAsync();
            var fileContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "avatar");
            var fullNameContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "FullName"); // ✅ viết hoa giống FE
            var birthDateContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "BirthDate"); // ✅ viết hoa giống FE
            var phoneContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "Phone"); // nếu FE gửi phone

            if (fullNameContent != null)
            {
                var fullName = await fullNameContent.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(fullName)) user.FullName = fullName;
            }

            if (phoneContent != null)
            {
                var phone = await phoneContent.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(phone)) user.Phone = phone;
            }

            if (birthDateContent != null)
            {
                var birthDateStr = await birthDateContent.ReadAsStringAsync();
                if (DateTime.TryParse(birthDateStr, out var bd)) user.BirthDate = bd; 
            }

            if (fileContent != null)
            {
                var stream = await fileContent.ReadAsStreamAsync();

                var account = new Account("dzrs9sv2n", "878186467936665", "ib72xVK9wDddH4FDp1o_UGdhZMI");
                var cloudinary = new Cloudinary(account);

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription("avatar", stream)
                };
                var uploadResult = cloudinary.Upload(uploadParams);

                user.AvatarUrl = uploadResult.SecureUrl.ToString();
            }

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
                    user.Phone,
                    user.BirthDate 
                }
            });
        }


    }
}
