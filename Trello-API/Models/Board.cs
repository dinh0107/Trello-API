using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public ICollection<List> Lists { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<BoardUser> BoardUsers { get; set; }
    }
    public class BoardUser
    {
        public int Id { get; set; }
        public int BoardId { get; set; }

        public int UserId { get; set; }

        public bool IsOwner { get; set; }
        public virtual User User { get; set; }
        public virtual Board Board { get; set; }
    }
}