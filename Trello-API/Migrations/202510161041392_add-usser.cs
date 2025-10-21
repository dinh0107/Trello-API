namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addusser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Cards", "CreatedById", c => c.Int(nullable: false));
            AddColumn("dbo.Cards", "CreatedAt", c => c.DateTime(nullable: false));
            CreateIndex("dbo.Cards", "CreatedById");
            AddForeignKey("dbo.Cards", "CreatedById", "dbo.Users", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Cards", "CreatedById", "dbo.Users");
            DropIndex("dbo.Cards", new[] { "CreatedById" });
            DropColumn("dbo.Cards", "CreatedAt");
            DropColumn("dbo.Cards", "CreatedById");
        }
    }
}
