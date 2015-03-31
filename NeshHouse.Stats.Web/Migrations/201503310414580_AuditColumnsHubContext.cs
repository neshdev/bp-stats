namespace NeshHouse.Stats.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuditColumnsHubContext : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserGroups", "CreateDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserGroups", "LastUpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserGroups", "IsConfirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserGroups", "IsConfirmed");
            DropColumn("dbo.UserGroups", "LastUpdatedDate");
            DropColumn("dbo.UserGroups", "CreateDate");
        }
    }
}
