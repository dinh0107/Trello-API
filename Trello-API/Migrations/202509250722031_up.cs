namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Phone", c => c.String());
            AddColumn("dbo.Users", "RefreshToken", c => c.String());
            AddColumn("dbo.Users", "Creatdate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Users", "RefreshTokenExpiry", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "RefreshTokenExpiry");
            DropColumn("dbo.Users", "Creatdate");
            DropColumn("dbo.Users", "RefreshToken");
            DropColumn("dbo.Users", "Phone");
        }
    }
}
