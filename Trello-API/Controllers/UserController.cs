using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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

            var form = httpRequest.Form;
            user.FullName = form["FullName"]?.Trim() ?? user.FullName;
            user.Phone = form["Phone"]?.Trim() ?? user.Phone;

            if (DateTime.TryParse(form["BirthDate"], out var parsedDate))
                user.BirthDate = parsedDate;

            if (httpRequest.Files.Count > 0)
            {
                var file = httpRequest.Files["avatar"];
                if (file != null && file.ContentLength > 0)
                {
                    var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var fileExt = Path.GetExtension(file.FileName).ToLower();

                    if (!validExtensions.Contains(fileExt))
                        return BadRequest("Chỉ hỗ trợ ảnh .jpg, .jpeg, .png, .webp");

                    var account = new Account(
                        ConfigurationManager.AppSettings["CloudinaryCloud"],
                        ConfigurationManager.AppSettings["CloudinaryApiKey"],
                        ConfigurationManager.AppSettings["CloudinaryApiSecret"]
                    );

                    var cloudinary = new Cloudinary(account);

                    var publicId = $"user_{user.Id}_{Guid.NewGuid()}";

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.InputStream),
                        Folder = "avatars",
                        PublicId = publicId,
                        Overwrite = true,
                        Transformation = new Transformation()
                            .Width(400).Height(400).Crop("fill").Gravity("face") 
                    };

                    var uploadResult = await cloudinary.UploadAsync(uploadParams);
                    user.AvatarUrl = uploadResult.SecureUrl?.ToString();
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
                    BirthDate = user.BirthDate?.ToString("yyyy-MM-dd")
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
