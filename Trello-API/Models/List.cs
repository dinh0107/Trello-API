using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trello_API.Models
{
    public class List
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Sort { get; set; }
        public int BoardId { get; set; }
        public Board Board { get; set; }
        public ICollection<Card> Cards { get; set; }
    }
}