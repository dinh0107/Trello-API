using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; }
    }
}