namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeuser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Email", c => c.String());
            DropColumn("dbo.Users", "Username");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Username", c => c.String());
            DropColumn("dbo.Users", "Email");
        }
    }
}
