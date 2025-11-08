namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class statuscard : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Cards", "IsDone", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Cards", "IsDone");
        }
    }
}
