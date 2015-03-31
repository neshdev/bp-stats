namespace NeshHouse.Stats.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Connections",
                c => new
                    {
                        ConnectionID = c.String(nullable: false, maxLength: 128),
                        UserAgent = c.String(),
                        Connected = c.Boolean(nullable: false),
                        User_Name = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ConnectionID)
                .ForeignKey("dbo.Users", t => t.User_Name)
                .Index(t => t.User_Name);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.UserGroups",
                c => new
                    {
                        UserName = c.String(nullable: false, maxLength: 128),
                        GroupName = c.String(nullable: false, maxLength: 128),
                        Team = c.String(),
                    })
                .PrimaryKey(t => new { t.UserName, t.GroupName })
                .ForeignKey("dbo.Groups", t => t.GroupName, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserName, cascadeDelete: true)
                .Index(t => t.UserName)
                .Index(t => t.GroupName);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserGroups", "UserName", "dbo.Users");
            DropForeignKey("dbo.Connections", "User_Name", "dbo.Users");
            DropForeignKey("dbo.UserGroups", "GroupName", "dbo.Groups");
            DropIndex("dbo.UserGroups", new[] { "GroupName" });
            DropIndex("dbo.UserGroups", new[] { "UserName" });
            DropIndex("dbo.Connections", new[] { "User_Name" });
            DropTable("dbo.Users");
            DropTable("dbo.UserGroups");
            DropTable("dbo.Groups");
            DropTable("dbo.Connections");
        }
    }
}
