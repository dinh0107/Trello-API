using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trello_API.Models;

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
        public int TargetListId { get; set; }
    }


    public class ChangeTitleRequest
    {
        public string Title { get; set; }
        public int Id { get; set; }
    }
    public class ChangeStatusRequest
    {
        public bool Status { get; set; }
        public int Id { get; set; }
    }

    public class CreateCardRequest
    {
        public string Title { get; set; }
        public int ListId { get; set; }
    }

    public class CardDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Sort { get; set; }
        public int ListId { get; set; }

    }
    public class CreateBoardRequest
    {
        public string Name { get; set; }
        public string BackgroundImage { get; set; }
        public bool IsPublic { get; set; }
    }
    public class CreateListRequest
    {
        public string Title { get; set; }
        public int BoardId { get; set; }
    }
    public class MoveCardRequestModel
    {
        public int CardId { get; set; }
        public int TargetListId { get; set; }
        public int NewSort { get; set; }
    }
    public class CreateCardStatusRequest
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int CardId { get; set; }
    }
    public class AddUserToCardRequest
    {
        public int CardId { get; set; }
        public int UserId { get; set; }
    }
    public class AddUserToBoardRequest
    {
        public int BoardId { get; set; }
        public List<int> UserIds { get; set; }
        public bool IsOwner { get; set; } = false;
    }

    public class CardDesRequest
    {
        public int CardId { get; set; }
        public string Description { get; set; }
    }
    public class AddUsersToCardRequest
    {
        public int CardId { get; set; }
        public int UserIds { get; set; }
    }
    public class UsersByCardRequest
    {
        public int CardId { get; set; }
        public List<User> Users { get; set; }
    }
}