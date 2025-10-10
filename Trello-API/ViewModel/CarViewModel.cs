using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.ViewModel
{
    public class MoveCardRequest
    {
        public int CardId { get; set; }
        public int NewListId { get; set; }
        public int NewSort { get; set; }
    }
    public class MoveListRequest
    {
        public int ListId { get; set; }
        public int NewSort { get; set; }
        public int BoardId { get; set; } 
    }
    public class CreateCardRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ListId { get; set; }
        public int? CardStatusId { get; set; }
        public int? AssigneeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class CreateBoardRequest
    {
        public string Name { get; set; }
        public string BackgroundImage { get; set; }
    }
    public class CreateListRequest
    {
        public string Title { get; set; }
        public int BoardId { get; set; }
    }
}