namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changecard : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CardStatus", "BoardId", "dbo.Boards");
            DropForeignKey("dbo.Cards", "CardStatusId", "dbo.CardStatus");
            DropIndex("dbo.CardStatus", new[] { "BoardId" });
            AddColumn("dbo.Cards", "CardStatus_Id", c => c.Int());
            AddColumn("dbo.CardStatus", "Card_Id", c => c.Int());
            CreateIndex("dbo.Cards", "CardStatus_Id");
            CreateIndex("dbo.CardStatus", "Card_Id");
            AddForeignKey("dbo.CardStatus", "Card_Id", "dbo.Cards", "Id");
            AddForeignKey("dbo.Cards", "CardStatus_Id", "dbo.CardStatus", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Cards", "CardStatus_Id", "dbo.CardStatus");
            DropForeignKey("dbo.CardStatus", "Card_Id", "dbo.Cards");
            DropIndex("dbo.CardStatus", new[] { "Card_Id" });
            DropIndex("dbo.Cards", new[] { "CardStatus_Id" });
            DropColumn("dbo.CardStatus", "Card_Id");
            DropColumn("dbo.Cards", "CardStatus_Id");
            CreateIndex("dbo.CardStatus", "BoardId");
            AddForeignKey("dbo.Cards", "CardStatusId", "dbo.CardStatus", "Id");
            AddForeignKey("dbo.CardStatus", "BoardId", "dbo.Boards", "Id", cascadeDelete: true);
        }
    }
}
