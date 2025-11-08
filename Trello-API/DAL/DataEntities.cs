using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Trello_API.Models;

namespace Trello_API.DAL
{
    public class DataEntities : DbContext
    {
        public DataEntities() : base("name=DataEntities") { }
        public DbSet<User> Users { get; set; }
        public DbSet<List>  Lists { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Comment>  Comments { get; set; }
        public DbSet<CardStatus> CardStatuses { get; set; }
        public DbSet<BoardUser> BoardUsers { get; set; }
        public DbSet<CardUser> CardUsers { get; set; }
    }
}