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

        [Authorize]
        [HttpGet, Route("my/{id}")]
        public IHttpActionResult GetBoard(int id)
        {
            var board = _unitOfWork.BoardRepository.GetById(id);
            if (board == null) return NotFound();

            var result = new
            {
                board.Id,
                board.Name,
                board.BackgroundImage,
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
                Name = model.Name,
                UserId = user.Id,
                BackgroundImage = selectedBackground
            };

            _unitOfWork.BoardRepository.Insert(board);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Board created successfully",
                data = new
                {
                    board.Id,
                    board.Name,
                    board.BackgroundImage
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

        [HttpDelete, Route("{id:int}")]
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

    }
}
