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

            var maxSort = _unitOfWork.CardRepository
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

            _unitOfWork.CardRepository.Insert(card);
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
            try
            {
                var cards = _unitOfWork.CardRepository
                    .GetQuery(c => c.ListId == listId, o => o.OrderBy(a => a.Sort))
                    .Select(c => new CardDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Sort = c.Sort,
                        ListId = c.ListId
                    })
                    .ToList();

                return Ok(cards);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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

            var card = _unitOfWork.CardRepository.GetById(request.CardId);
            if (card == null)
                return NotFound();

            card.ListId = request.TargetListId;
            card.Sort = request.NewSort;
            _unitOfWork.CardRepository.Update(card);

            var cardsInTarget = _unitOfWork.CardRepository
                .GetQuery(c => c.ListId == request.TargetListId && c.Id != card.Id)
                .OrderBy(c => c.Sort)
                .ToList();

            int index = 0;
            foreach (var c in cardsInTarget)
            {
                if (index == request.NewSort) index++;
                c.Sort = index;
                _unitOfWork.CardRepository.Update(c);
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

            var card = _unitOfWork.CardRepository.GetById(id);
            if (card == null)
                return NotFound();

            var result = new
            {
                card.Id,
                card.Title,
                card.Description,
                card.CreatedAt,
                CreatedBy = new
                {
                    card.CreatedBy.Id,
                    card.CreatedBy.FullName,
                    card.CreatedBy.Email,
                },
                ListId = card.ListId
            };

            return Ok(result);
        }


        [HttpDelete, Route("delete-card/{id}")]
        public IHttpActionResult DelelteCard(int id)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null)
                return Unauthorized();

            var card = _unitOfWork.CardRepository.GetById(id);
            if (card == null)
                return NotFound();

            _unitOfWork.CardRepository.Delete(card);
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
      


        [HttpPut, Route("description-card")]
        public IHttpActionResult DescriptionCard([FromBody] CardDesRequest request)
        {
            if (request == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (currentUser == null)
                return Unauthorized();

            var card = _unitOfWork.CardRepository.GetById(request.CardId);
            if (card == null)
                return NotFound();

            card.Description = request.Description;
            _unitOfWork.CardRepository.Update(card);
            _unitOfWork.Save();
            return Ok(new
            {
                Success = true,
                Message = "Thêm mô tả card thành công",
                card = new 
                {
                    card.Description,
                }
            });
        }

        [HttpPost]
        [Route("add-user")]
        public IHttpActionResult AddUserToCard([FromBody] AddUserToCardRequest request)
        {
            if (request == null || request.CardId <= 0 || request.UserId <= 0)
                return BadRequest("Dữ liệu không hợp lệ. CardId và UserId là bắt buộc.");

            var card = _unitOfWork.CardRepository.GetById(request.CardId);
            if (card == null)
                return NotFound();

            var user = _unitOfWork.UserRepository.GetById(request.UserId);
            if (user == null)
                return NotFound();

            var exists = _unitOfWork.CardUserRepository
                .GetQuery(cu => cu.CardId == request.CardId && cu.UserId == request.UserId)
                .Any();

            if (exists)
                return Content(HttpStatusCode.Conflict, new
                {
                    Success = false,
                    Message = "Người dùng này đã tồn tại trong card."
                });

            var cardUser = new CardUser
            {
                CardId = request.CardId,
                UserId = request.UserId,
                JoinedAt = DateTime.UtcNow
            };

            _unitOfWork.CardUserRepository.Insert(cardUser);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Thêm người dùng vào card thành công",
                Data = new
                {
                    card.Id,
                    card.Title,
                    User = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.AvatarUrl
                    }
                }
            });
        }

        [HttpPost]
        [Route("remove-user")]
        public IHttpActionResult RemoveUserToCard([FromBody] AddUserToCardRequest request)
        {
            if (request == null || request.CardId <= 0 || request.UserId <= 0)
                return BadRequest("Dữ liệu không hợp lệ. CardId và UserId là bắt buộc.");

            var card = _unitOfWork.CardRepository.GetById(request.CardId);
            if (card == null)
                return NotFound();

            var user = _unitOfWork.UserRepository.GetById(request.UserId);
            if (user == null)
                return NotFound();

            var exists = _unitOfWork.CardUserRepository
                .GetQuery(cu => cu.CardId == request.CardId && cu.UserId == request.UserId)
                .Any();

            if (exists)
                return Content(HttpStatusCode.Conflict, new
                {
                    Success = false,
                    Message = "Người dùng này đã tồn tại trong card."
                });

            var cardUser = new CardUser
            {
                CardId = request.CardId,
                UserId = request.UserId,
                JoinedAt = DateTime.UtcNow
            };

            _unitOfWork.CardUserRepository.Delete(cardUser);
            _unitOfWork.Save();

            return Ok(new
            {
                Success = true,
                Message = "Xóa người dùng thành công",
                Data = new
                {
                    card.Id,
                    card.Title,
                    User = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                    }
                }
            });
        }

        [HttpGet]
        [Route("{cardId}/users")]
        public IHttpActionResult GetUsersByCard(int cardId)
        {
            var card = _unitOfWork.CardRepository.GetById(cardId);
            if (card == null)
                return NotFound();

            var users = _unitOfWork.CardUserRepository
                .GetQuery(cu => cu.CardId == cardId)
                .Select(cu => cu.User)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.AvatarUrl
                })
                .ToList();

            return Ok(new
            {
                Success = true,
                Message = "Lấy danh sách người dùng trong card thành công",
                Data = users
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

            var card = _unitOfWork.CardRepository.GetById(request.Id);
            if (card == null) return NotFound();

            card.Title = request.Title;
            _unitOfWork.Save();
            return Ok(new
            {
                Success = true,
                Message = "Cập nhật thành công",
                Title = request.Title,
            });
        }
        [HttpPut, Route("change-status")]
        public IHttpActionResult ChangeStatus([FromBody] ChangeStatusRequest request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ");

            var identity = (ClaimsIdentity)User.Identity;
            var user = _unitOfWork.UserRepository
                .GetQuery(u => u.Email == identity.Name)
                .FirstOrDefault();

            if (user == null) return Unauthorized();

            int userId = user.Id;

            var card = _unitOfWork.CardRepository.GetById(request.Id);
            if (card == null) return NotFound();

            card.IsDone = request.Status;
            _unitOfWork.Save();
            return Ok(new
            {
                Success = true,
                Message = "Cập nhật thành công",
                Card = new
                {
                    Id = card.Id,
                    Title = card.Title,
                    Status = card.IsDone
                },
            });
        }
        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
