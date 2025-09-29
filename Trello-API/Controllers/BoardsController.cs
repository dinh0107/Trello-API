using Microsoft.Ajax.Utilities;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
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

        [HttpGet, Route("my")]
        public IHttpActionResult GetMyBoards()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var boards = _unitOfWork.BoardUserRepository
                .GetQuery(bu => bu.UserId == userId)
                .Select(bu => bu.Board)
                .ToList();
            return Ok(boards);
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetBoard(int id)
        {
            var board = _unitOfWork.BoardRepository.GetById(id);
            if (board == null) return NotFound();
            return Ok(board);
        }

        [HttpPost, Route("create-board")]
        public IHttpActionResult CreateBoard([FromBody] CreateBoardRequest request)
        {
            if (request == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var board = new Board
            {
                Name = request.Name,
                UserId = userId,
            };

            _unitOfWork.BoardRepository.Insert(board);
            _unitOfWork.Save();

            var boardUser = new BoardUser
            {
                BoardId = board.Id,
                UserId = userId,
                IsOwner = true
            };
            _unitOfWork.BoardUserRepository.Insert(boardUser);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Tạo board thành công",
                Board = new
                {
                    board.Id,
                    board.Name,
                    board.UserId,

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


        [HttpPost, Route("{boardId:int}/members")]
        public IHttpActionResult AddMember(int boardId, [FromBody] int userId)
        {
            var board = _unitOfWork.BoardRepository.GetById(boardId);
            if (board == null) return NotFound();

            var user = _unitOfWork.UserRepository.GetById(userId);
            if (user == null) return NotFound();

            var boardUser = new BoardUser
            {
                BoardId = boardId,
                UserId = userId,
                IsOwner = false
            };
            _unitOfWork.BoardUserRepository.Insert(boardUser);
            _unitOfWork.Save();

            return Ok(new { Success = true, Message = "Thêm thành viên thành công" });
        }
        [HttpGet, Route("{boardId:int}/members")]
        public IHttpActionResult GetMembers(int boardId)
        {
            var members = _unitOfWork.BoardUserRepository
                .GetQuery(bu => bu.BoardId == boardId)
                .Select(bu => new { bu.UserId, bu.User.Email, bu.IsOwner })
                .ToList();

            return Ok(members);
        }
    }
}
