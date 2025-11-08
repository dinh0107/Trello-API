namespace Trello_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class addusercard : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Cards", "AssigneeId", "dbo.Users");
            DropIndex("dbo.Cards", new[] { "AssigneeId" });

            CreateTable(
                "dbo.CardUsers",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CardId = c.Int(nullable: false),
                    UserId = c.Int(nullable: false),
                    JoinedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                // ⚠️ Đặt cascadeDelete = false để tránh multiple cascade paths
                .ForeignKey("dbo.Cards", t => t.CardId, cascadeDelete: false)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: false)
                .Index(t => t.CardId)
                .Index(t => t.UserId);

            DropColumn("dbo.Cards", "AssigneeId");
        }

        public override void Down()
        {
            AddColumn("dbo.Cards", "AssigneeId", c => c.Int());
            DropForeignKey("dbo.CardUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.CardUsers", "CardId", "dbo.Cards");
            DropIndex("dbo.CardUsers", new[] { "UserId" });
            DropIndex("dbo.CardUsers", new[] { "CardId" });
            DropTable("dbo.CardUsers");
            CreateIndex("dbo.Cards", "AssigneeId");
            AddForeignKey("dbo.Cards", "AssigneeId", "dbo.Users", "Id");
        }
    }
}
