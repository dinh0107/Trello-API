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
    [RoutePrefix("api/lists")]
    public class ListsController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();


        [HttpGet, Route("my")]
        public IHttpActionResult GetMyLists()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var lists = _unitOfWork.ListRepository
             .GetQuery(l => l.Board.BoardUsers.Any(bu => bu.UserId == userId))
             .OrderBy(l => l.Sort)
             .ToList();
            return Ok(lists);
        }

        [HttpPost, Route("create-list")]
        public IHttpActionResult CreateList([FromBody] CreateListRequest request)
        {
            if (request == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var board = _unitOfWork.BoardRepository.GetById(request.BoardId);
            if (board == null)
                return NotFound();
            var maxSort = _unitOfWork.ListRepository
                            .GetQuery(l => l.BoardId == request.BoardId)
                            .Select(l => (int?)l.Sort)
                            .Max() ?? 0;

            var list = new List
            {
                Title = request.Title,
                BoardId = request.BoardId,
                Sort = maxSort + 1
            };

            _unitOfWork.ListRepository.Insert(list);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Tạo list thành công",
                List = new
                {
                    list.Id,
                    list.Title,
                    list.Sort,
                    list.BoardId
                }
            });
        }

        [HttpGet, Route("board/{boardId:int}")]
        public IHttpActionResult GetListsByBoard(int boardId)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            int userId = int.Parse(userIdClaim);

          
            var lists = _unitOfWork.ListRepository
                .GetQuery(l => l.BoardId == boardId)
                .OrderBy(l => l.Sort)
                .Select(l => new {
                    l.Id,
                    l.Title,
                    l.Sort,
                    l.BoardId
                })
                .ToList();

            return Ok(lists);
        }
        [HttpPut, Route("move")]
        public IHttpActionResult MoveList([FromBody] MoveListRequest request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var list = _unitOfWork.ListRepository.GetById(request.ListId);
            if (list == null) return NotFound();

            
            list.Sort = request.NewSort;

            _unitOfWork.ListRepository.Update(list);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Cập nhật vị trí list thành công",
                List = new
                {
                    list.Id,
                    list.Title,
                    list.Sort,
                    list.BoardId
                }
            });
        }
    }

}
