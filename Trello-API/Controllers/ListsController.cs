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
        public IHttpActionResult GetMyLists(int boadId)
        {
            try
            {
                var identity = (ClaimsIdentity)User.Identity;
                var user = _unitOfWork.UserRepository
                    .GetQuery(u => u.Email == identity.Name)
                    .FirstOrDefault();

                if (user == null)
                    return Unauthorized();

                var lists = _unitOfWork.ListRepository
                    .GetQuery(l => l.BoardId == boadId)
                    .OrderBy(l => l.Sort)
                    .ToList();

                return Ok(lists);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("create-list")]
        public IHttpActionResult CreateList([FromBody] CreateListRequest request)
        {
            if (request == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return Unauthorized();

            int userId = user.Id;

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
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return Unauthorized();

            int userId = user.Id;

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
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return Unauthorized();

            int userId = user.Id;

            var list = _unitOfWork.ListRepository.GetById(request.ListId);
            if (list == null) return NotFound();

            var targetList = _unitOfWork.ListRepository.GetById(request.TargetListId);
            if (targetList == null) return BadRequest("Không tìm thấy list đích");

            var tempSort = list.Sort;
            list.Sort = targetList.Sort;
            targetList.Sort = tempSort;

            _unitOfWork.ListRepository.Update(list);
            _unitOfWork.ListRepository.Update(targetList);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Hoán đổi vị trí list thành công",
                Lists = new[]
                {
            new { list.Id, list.Title, list.Sort, list.BoardId },
            new { targetList.Id, targetList.Title, targetList.Sort, targetList.BoardId }
        }
            });
        }

        [HttpPut, Route("change-title")]
        public IHttpActionResult ChangeTitle([FromBody] ChangeTitleRequest request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return Unauthorized();

            int userId = user.Id;

            var list = _unitOfWork.ListRepository.GetById(request.Id);
            if (list == null) return NotFound();

            list.Title = request.Title;
            _unitOfWork.Save();
            return Ok(new
            {
                Success = true,
                Message = "Cập nhật thành công",
            });
        }
        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }

}
