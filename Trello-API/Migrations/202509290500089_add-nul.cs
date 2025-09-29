namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addnul : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "RefreshTokenExpiry", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "RefreshTokenExpiry", c => c.DateTime(nullable: false));
        }
    }
}
