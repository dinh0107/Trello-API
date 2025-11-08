using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Ajax.Utilities;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Trello_API.DAL;
using Trello_API.Models;
using Trello_API.ViewModel;

namespace Trello_API.Controllers
{
    [Authorize]
    [RoutePrefix("api/boards")]
    public class BoardsController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        [Authorize]
        [HttpGet]
        [Route("my")]
        public IHttpActionResult GetMyBoards()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return NotFound();

            var boards = _unitOfWork.BoardRepository
                .GetQuery(b => b.UserId == user.Id, o => o.OrderBy(b => b.Id))
                .ToList();

            var boardDtos = boards.Select(b => new BoardDto
            {
                Id = b.Id,
                Name = b.Name,
                BackgroundImage = b.BackgroundImage
            }).ToList();

            return Ok(boardDtos);
        }

        [AllowAnonymous]
        [HttpGet, Route("my/{id}")]
        public IHttpActionResult GetBoard(int id)
        {
            var board = _unitOfWork.BoardRepository.GetById(id);

            if (board == null)
                return NotFound();

            if (board.IsPublic)
            {
                return Ok(new
                {
                    board.Id,
                    board.Name,
                    board.BackgroundImage,
                    IsPublic = board.IsPublic,
                    Lists = board.Lists?.Select(l => new
                    {
                        l.Id,
                        l.Title
                    })
                });
            }

            if (HttpContext.Current.User?.Identity?.IsAuthenticated != true)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    Success = false,
                    Message = "Bạn cần đăng nhập để xem board này"
                });
            }

            var email = HttpContext.Current.User.Identity.Name;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == email)
                .FirstOrDefault();

            if (user == null)
                return Content(HttpStatusCode.Unauthorized, new
                {
                    Success = false,
                    Message = "Không xác định được người dùng"
                });

            bool isOwner = board.UserId == user.Id;
            bool isMember = board.BoardUsers?.Any(bu => bu.UserId == user.Id) ?? false;

            if (!isOwner && !isMember)
            {
                return Content(HttpStatusCode.Forbidden, new
                {
                    Success = false,
                    Message = "Bạn không có quyền truy cập board này"
                });
            }

            var result = new
            {
                board.Id,
                board.Name,
                board.BackgroundImage,
                IsPublic = board.IsPublic,
                Lists = board.Lists?.Select(l => new
                {
                    l.Id,
                    l.Title
                })
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("backgrounds")]
        public IHttpActionResult GetAvailableBackgrounds()
        {
            var backgrounds = new List<string>
            {
                "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-1.jpg",
                "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-2.jpg",
                "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-3.webp",
                "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-4.jpg"
            };
            return Ok(backgrounds);
        }

        [HttpPost]
        [Route("create")]
        public IHttpActionResult CreateBoard([FromBody] CreateBoardRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Board name is required");

            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return Unauthorized();

            var allowedBackgrounds = new List<string>
                {
                    "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-1.jpg",
                    "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-2.jpg",
                    "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-3.webp",
                    "https://res.cloudinary.com/dzrs9sv2n/image/upload/v1759303170/bg-4.jpg"
                };

            string selectedBackground = allowedBackgrounds.Contains(model.BackgroundImage)
                ? model.BackgroundImage
                : allowedBackgrounds[0];

            var board = new Board
            {
                Name = model.Name.Trim(),
                UserId = user.Id,
                IsPublic = model.IsPublic,
                BackgroundImage = selectedBackground,
            };

            _unitOfWork.BoardRepository.Insert(board);
            _unitOfWork.Save();

            var boardUser = new BoardUser
            {
                BoardId = board.Id,
                UserId = user.Id,
                IsOwner = true
            };

            _unitOfWork.BoardUserRepository.Insert(boardUser);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Tạo board thành công",
                data = new
                {
                    board.Id,
                    board.Name,
                    board.BackgroundImage,
                    Owner = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.AvatarUrl
                    }
                }
            });
        }


        [HttpPut, Route("{id:int}")]
        public IHttpActionResult UpdateBoard(int id, [FromBody] Board board)
        {
            var existing = _unitOfWork.BoardRepository.GetById(id);
            if (existing == null) return NotFound();

            existing.Name = board.Name;
            _unitOfWork.BoardRepository.Update(existing);
            _unitOfWork.Save();
            return Ok(existing);
        }

        [HttpDelete, Route("delete-board/{id}")]
        public IHttpActionResult DeleteBoard(int id)
        {
            var board = _unitOfWork.BoardRepository.GetById(id);
            if (board == null) return NotFound();

            _unitOfWork.BoardRepository.Delete(board);
            _unitOfWork.Save();
            return Ok();
        }


        [HttpGet]
        [Route("search")]
        public IHttpActionResult SearchUser(string keyword = "")
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(Enumerable.Empty<object>());
            keyword = keyword.Trim().ToLower();
            var users = _unitOfWork.UserRepository
                .GetQuery(u =>
                    u.FullName.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword))
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.AvatarUrl,
                    u.Phone,
                })
                .Take(10)
                .ToList();

            return Ok(users);
        }
        [HttpPost, Route("{boardId:int}/members")]
        public IHttpActionResult AddMember(int boardId, [FromBody] int userId)
        {
            var board = _unitOfWork.BoardRepository.GetById(boardId);
            if (board == null)
                return NotFound();

            var user = _unitOfWork.UserRepository.GetById(userId);
            if (user == null)
                return NotFound();

            // Kiểm tra đã tồn tại chưa
            var existingMember = _unitOfWork.BoardUserRepository
                .GetQuery(bu => bu.BoardId == boardId && bu.UserId == userId)
                .FirstOrDefault();

            if (existingMember != null)
            {
                return BadRequest("Thành viên này đã có trong board");
            }

            var boardUser = new BoardUser
            {
                BoardId = boardId,
                UserId = userId,
                IsOwner = false
            };

            _unitOfWork.BoardUserRepository.Insert(boardUser);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Thêm thành viên thành công",
                Member = new
                {
                    boardUser.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsOwner = boardUser.IsOwner
                }
            });
        }

        [HttpGet, Route("{boardId:int}/members")]
        public IHttpActionResult GetMembers(int boardId)
        {
            var members = _unitOfWork.BoardUserRepository
                .GetQuery(bu => bu.BoardId == boardId && bu.User != null)
                .Select(bu => new
                {
                    bu.UserId,
                    Email = bu.User.Email,
                    FullName = bu.User.FullName,
                    AvatarUrl = bu.User.AvatarUrl,
                    bu.IsOwner
                })
                .ToList();

            if (!members.Any())
                return NotFound();

            return Ok(members);
        }
        [HttpPost, Route("add-user")]
        public IHttpActionResult AddUserToBoard([FromBody] AddUserToBoardRequest request)
        {
            if (request == null || request.UserIds == null || !request.UserIds.Any())
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (currentUser == null)
                return Unauthorized();

            var board = _unitOfWork.BoardRepository.GetById(request.BoardId);
            if (board == null)
                return BadRequest("Không tìm thấy board.");

            var addedMembers = new List<object>();

            foreach (var userId in request.UserIds)
            {
                var user = _unitOfWork.UserRepository.GetById(userId);
                if (user == null)
                    continue;

                var existing = _unitOfWork.BoardUserRepository
                    .GetQuery(bu => bu.BoardId == request.BoardId && bu.UserId == userId)
                    .FirstOrDefault();

                if (existing != null)
                    continue;

                var boardUser = new BoardUser
                {
                    BoardId = request.BoardId,
                    UserId = userId,
                    IsOwner = false,
                };

                _unitOfWork.BoardUserRepository.Insert(boardUser);

                addedMembers.Add(new
                {
                    boardUser.Id,
                    boardUser.BoardId,
                    boardUser.UserId,
                    IsOwner = false,
                    User = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email
                    }
                });
            }

            _unitOfWork.Save();

            if (!addedMembers.Any())
                return BadRequest("Không có user nào được thêm (tất cả đều tồn tại hoặc không hợp lệ).");

            return Ok(new
            {
                Success = true,
                Message = $"Đã thêm {addedMembers.Count} user vào board thành công",
                Members = addedMembers
            });
        }
        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }

    }
}
