using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
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

        [HttpPost]
        [Route("update-info")]
        public async Task<IHttpActionResult> UpdateCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null)
                return NotFound();

            var httpRequest = HttpContext.Current.Request;

            // Lấy dữ liệu từ form
            var fullName = httpRequest.Form["FullName"];
            var phone = httpRequest.Form["Phone"];
            var birthDate = httpRequest.Form["BirthDate"];

            if (!string.IsNullOrWhiteSpace(fullName))
                user.FullName = fullName.Trim();

            if (!string.IsNullOrWhiteSpace(phone))
                user.Phone = phone.Trim();

            if (DateTime.TryParse(birthDate, out var bd))
                user.BirthDate = bd;

            // Xử lý file
            if (httpRequest.Files.Count > 0)
            {
                var file = httpRequest.Files["avatar"]; 
                if (file != null && file.ContentLength > 0)
                {
                    using (var stream = file.InputStream)
                    {
                        var account = new Account(
                            ConfigurationManager.AppSettings["CloudinaryCloud"],
                            ConfigurationManager.AppSettings["CloudinaryApiKey"],
                            ConfigurationManager.AppSettings["CloudinaryApiSecret"]
                        );

                        var cloudinary = new Cloudinary(account);
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(file.FileName, stream),
                            Folder = "avatars",
                            PublicId = $"user_{user.Id}_{Guid.NewGuid()}",
                            Overwrite = true
                        };

                        var uploadResult = await cloudinary.UploadAsync(uploadParams);
                        user.AvatarUrl = uploadResult.SecureUrl?.ToString();
                    }
                }
            }

            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Cập nhật thông tin thành công",
                User = new
                {
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
