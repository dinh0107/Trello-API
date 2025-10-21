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
    [RoutePrefix("api/cards")]
    public class CardsController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        [HttpPost, Route("add")]
        public IHttpActionResult CreateCard([FromBody] CreateCardRequest request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ");
            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return Unauthorized();
            var list = _unitOfWork.ListRepository.GetById(request.ListId);
            if (list == null)
            {
                return NotFound();
            }

            var maxSort = _unitOfWork.CardRepositoryRepository
                .GetQuery(c => c.ListId == list.Id)
                .Select(c => (int?)c.Sort)
                .Max() ?? 0;

            var card = new Card
            {
                Title = request.Title,
                ListId = request.ListId,
                Sort = maxSort + 1,
                CreatedById = user.Id,
                CreatedBy = user
            };

            _unitOfWork.CardRepositoryRepository.Insert(card);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Tạo card thành công",
                Card = new
                {
                    card.Id,
                    card.Title,
                    card.Description,
                    card.ListId,
                    card.Sort
                }
            });
        }

        [HttpGet, Route("list/{listId:int}")]
        public IHttpActionResult GetCardsByList(int listId)
        {
            var cards = _unitOfWork.CardRepositoryRepository.GetQuery(c => c.ListId == listId, o => o.OrderBy(a => a.Sort)).ToList();
            return Ok(cards);
        }


        [HttpPut, Route("move")]
        public IHttpActionResult MoveCard([FromBody] MoveCardRequestModel request)
        {
            if (request == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null)
                return Unauthorized();

            var card = _unitOfWork.CardRepositoryRepository.GetById(request.CardId);
            if (card == null)
                return NotFound();

            card.ListId = request.TargetListId;
            card.Sort = request.NewSort;
            _unitOfWork.CardRepositoryRepository.Update(card);

            var cardsInTarget = _unitOfWork.CardRepositoryRepository
                .GetQuery(c => c.ListId == request.TargetListId && c.Id != card.Id)
                .OrderBy(c => c.Sort)
                .ToList();

            int index = 0;
            foreach (var c in cardsInTarget)
            {
                if (index == request.NewSort) index++;
                c.Sort = index;
                _unitOfWork.CardRepositoryRepository.Update(c);
                index++;
            }

            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Cập nhật vị trí card thành công",
                Card = new
                {
                    card.Id,
                    card.Title,
                    card.ListId,
                    card.Sort
                }
            });
        }
        [HttpGet, Route("get-card/{id}")]
        public IHttpActionResult GetCardById(int id)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null)
                return Unauthorized();

            var card = _unitOfWork.CardRepositoryRepository.GetById(id);
            if (card == null)
                return NotFound();

            return Ok(card);
        }

        [HttpPost, Route("delete-card/{id}")]
        public IHttpActionResult DelelteCard(int id)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null)
                return Unauthorized();

            var card = _unitOfWork.CardRepositoryRepository.GetById(id);
            if (card == null)
                return NotFound();

            _unitOfWork.CardRepositoryRepository.Delete(card);
            _unitOfWork.Save();
            return Ok(
                new
                {
                    Success = true,
                    Message = "Xóa thành card thành công",
                }
            );
        }
        [HttpPost, Route("create-status")]
        public IHttpActionResult CreateCardStatus([FromBody] CreateCardStatusRequest request)
        {
            if (request == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null)
                return Unauthorized();

            var card = _unitOfWork.BoardRepository.GetById(request.CardId);
            if (card == null)
                return BadRequest("Không tìm thấy card.");

            var status = new CardStatus
            {
                Name = request.Name,
                Color = request.Color,
                CardId = request.CardId
            };

            _unitOfWork.CardStatusRepository.Insert(status);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Tạo trạng thái card thành công",
                CardStatus = new
                {
                    status.Id,
                    status.Name,
                    status.Color,
                    status.CardId
                }
            });
        }
        [HttpPut, Route("add-user")]
        public IHttpActionResult AddUserToCard([FromBody] AddUserToCardRequest request)
        {
            if (request == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (currentUser == null)
                return Unauthorized();

            var card = _unitOfWork.CardRepositoryRepository.GetById(request.CardId);
            if (card == null)
                return NotFound();

            var list = _unitOfWork.ListRepository.GetById(card.ListId);
            if (list == null)
                return BadRequest("Không tìm thấy list chứa card.");

            var boardId = list.BoardId;
            var userInBoard = _unitOfWork.BoardUserRepository
                .GetQuery(m => m.BoardId == boardId && m.UserId == request.UserId)
                .FirstOrDefault();

            if (userInBoard == null)
                return BadRequest("Người dùng này không nằm trong board.");

            card.AssigneeId = request.UserId;
            _unitOfWork.CardRepositoryRepository.Update(card);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Thêm user vào card thành công",
                Card = new
                {
                    card.Id,
                    card.Title,
                    card.AssigneeId,
                    AssigneeName = _unitOfWork.UserRepository.GetById(request.UserId)?.FullName
                }
            });
        }

    }
}
