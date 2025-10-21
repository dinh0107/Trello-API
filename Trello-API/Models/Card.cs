using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Sort { get; set; } 
        public string Description { get; set; }
        public int ListId { get; set; }
        public List List { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AssigneeId { get; set; }
        public int? CardStatusId { get; set; }
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User Assignee { get; set; }
        public virtual CardStatus CardStatus { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
    public class CardStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Color { get; set; } 
        public int CardId { get; set; }
        public virtual Card Card { get; set; }
        public virtual ICollection<Card> Cards { get; set; }
    }
}