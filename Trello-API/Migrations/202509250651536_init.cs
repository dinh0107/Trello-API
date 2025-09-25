namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Boards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Lists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        BoardId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Boards", t => t.BoardId, cascadeDelete: true)
                .Index(t => t.BoardId);
            
            CreateTable(
                "dbo.Cards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        ListId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lists", t => t.ListId, cascadeDelete: true)
                .Index(t => t.ListId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        CardId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cards", t => t.CardId, cascadeDelete: true)
                .Index(t => t.CardId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(),
                        AvatarUrl = c.String(),
                        Username = c.String(),
                        PasswordHash = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserBoards",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        Board_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Board_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Boards", t => t.Board_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Board_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserBoards", "Board_Id", "dbo.Boards");
            DropForeignKey("dbo.UserBoards", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Cards", "ListId", "dbo.Lists");
            DropForeignKey("dbo.Comments", "CardId", "dbo.Cards");
            DropForeignKey("dbo.Lists", "BoardId", "dbo.Boards");
            DropIndex("dbo.UserBoards", new[] { "Board_Id" });
            DropIndex("dbo.UserBoards", new[] { "User_Id" });
            DropIndex("dbo.Comments", new[] { "CardId" });
            DropIndex("dbo.Cards", new[] { "ListId" });
            DropIndex("dbo.Lists", new[] { "BoardId" });
            DropTable("dbo.UserBoards");
            DropTable("dbo.Users");
            DropTable("dbo.Comments");
            DropTable("dbo.Cards");
            DropTable("dbo.Lists");
            DropTable("dbo.Boards");
        }
    }
}
