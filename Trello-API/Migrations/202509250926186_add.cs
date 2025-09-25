namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserBoards", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserBoards", "Board_Id", "dbo.Boards");
            DropIndex("dbo.UserBoards", new[] { "User_Id" });
            DropIndex("dbo.UserBoards", new[] { "Board_Id" });
            CreateTable(
                "dbo.BoardUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BoardId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        IsOwner = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Boards", t => t.BoardId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.BoardId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.CardStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Color = c.String(),
                        BoardId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Boards", t => t.BoardId, cascadeDelete: true)
                .Index(t => t.BoardId);
            
            AddColumn("dbo.Boards", "UserId", c => c.Int(nullable: false));
            AddColumn("dbo.Lists", "Sort", c => c.Int(nullable: false));
            AddColumn("dbo.Cards", "Sort", c => c.Int(nullable: false));
            AddColumn("dbo.Cards", "StartDate", c => c.DateTime());
            AddColumn("dbo.Cards", "EndDate", c => c.DateTime());
            AddColumn("dbo.Cards", "AssigneeId", c => c.Int());
            AddColumn("dbo.Cards", "CardStatusId", c => c.Int());
            CreateIndex("dbo.Boards", "UserId");
            CreateIndex("dbo.Cards", "AssigneeId");
            CreateIndex("dbo.Cards", "CardStatusId");
            AddForeignKey("dbo.Boards", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Cards", "AssigneeId", "dbo.Users", "Id");
            AddForeignKey("dbo.Cards", "CardStatusId", "dbo.CardStatus", "Id");
            DropTable("dbo.UserBoards");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserBoards",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        Board_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Board_Id });
            
            DropForeignKey("dbo.Cards", "CardStatusId", "dbo.CardStatus");
            DropForeignKey("dbo.CardStatus", "BoardId", "dbo.Boards");
            DropForeignKey("dbo.Cards", "AssigneeId", "dbo.Users");
            DropForeignKey("dbo.BoardUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.Boards", "UserId", "dbo.Users");
            DropForeignKey("dbo.BoardUsers", "BoardId", "dbo.Boards");
            DropIndex("dbo.CardStatus", new[] { "BoardId" });
            DropIndex("dbo.Cards", new[] { "CardStatusId" });
            DropIndex("dbo.Cards", new[] { "AssigneeId" });
            DropIndex("dbo.BoardUsers", new[] { "UserId" });
            DropIndex("dbo.BoardUsers", new[] { "BoardId" });
            DropIndex("dbo.Boards", new[] { "UserId" });
            DropColumn("dbo.Cards", "CardStatusId");
            DropColumn("dbo.Cards", "AssigneeId");
            DropColumn("dbo.Cards", "EndDate");
            DropColumn("dbo.Cards", "StartDate");
            DropColumn("dbo.Cards", "Sort");
            DropColumn("dbo.Lists", "Sort");
            DropColumn("dbo.Boards", "UserId");
            DropTable("dbo.CardStatus");
            DropTable("dbo.BoardUsers");
            CreateIndex("dbo.UserBoards", "Board_Id");
            CreateIndex("dbo.UserBoards", "User_Id");
            AddForeignKey("dbo.UserBoards", "Board_Id", "dbo.Boards", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserBoards", "User_Id", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
