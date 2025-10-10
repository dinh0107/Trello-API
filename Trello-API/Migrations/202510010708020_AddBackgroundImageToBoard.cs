namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBackgroundImageToBoard : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Boards", "BackgroundImage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Boards", "BackgroundImage");
        }
    }
}
