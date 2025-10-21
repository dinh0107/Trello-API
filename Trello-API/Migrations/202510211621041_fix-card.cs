namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixcard : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CardStatus", "Card_Id", "dbo.Cards");
            DropIndex("dbo.CardStatus", new[] { "Card_Id" });
            RenameColumn(table: "dbo.CardStatus", name: "Card_Id", newName: "CardId");
            AlterColumn("dbo.CardStatus", "CardId", c => c.Int(nullable: false));
            CreateIndex("dbo.CardStatus", "CardId");
            AddForeignKey("dbo.CardStatus", "CardId", "dbo.Cards", "Id", cascadeDelete: true);
            DropColumn("dbo.CardStatus", "BoardId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CardStatus", "BoardId", c => c.Int(nullable: false));
            DropForeignKey("dbo.CardStatus", "CardId", "dbo.Cards");
            DropIndex("dbo.CardStatus", new[] { "CardId" });
            AlterColumn("dbo.CardStatus", "CardId", c => c.Int());
            RenameColumn(table: "dbo.CardStatus", name: "CardId", newName: "Card_Id");
            CreateIndex("dbo.CardStatus", "Card_Id");
            AddForeignKey("dbo.CardStatus", "Card_Id", "dbo.Cards", "Id");
        }
    }
}
