using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        [HttpPost, Route("")]
        public IHttpActionResult CreateCard([FromBody] CreateCardRequest request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ");

            var maxSort = _unitOfWork.CardRepositoryRepository
                .GetQuery(c => c.ListId == request.ListId)
                .Select(c => (int?)c.Sort)
                .Max() ?? 0;

            var card = new Card
            {
                Title = request.Title,
                Description = request.Description,
                ListId = request.ListId,
                CardStatusId = request.CardStatusId,
                AssigneeId = request.AssigneeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Sort = maxSort + 1
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
                    card.CardStatusId,
                    card.AssigneeId,
                    card.StartDate,
                    card.EndDate,
                    card.Sort
                }
            });
        }

        [HttpGet, Route("list/{listId:int}")]
        public IHttpActionResult GetCardsByList(int listId)
        {
            var cards = _unitOfWork.CardRepositoryRepository.GetQuery(c => c.ListId == listId).ToList();
            return Ok(cards);
        }
    }
}
