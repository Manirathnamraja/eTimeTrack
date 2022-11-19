namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTypes5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectUserTypes", "AliasCode", c => c.String());
            AddColumn("dbo.ProjectUserTypes", "AliasType", c => c.String());
            AddColumn("dbo.ProjectUserTypes", "AliasDescription", c => c.String());
            AddColumn("dbo.UserTypes", "Type", c => c.String());
            AddColumn("dbo.UserTypes", "Description", c => c.String());
            DropColumn("dbo.ProjectUserTypes", "AliasName");
            DropColumn("dbo.ProjectUserTypes", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProjectUserTypes", "Description", c => c.String());
            AddColumn("dbo.ProjectUserTypes", "AliasName", c => c.String());
            DropColumn("dbo.UserTypes", "Description");
            DropColumn("dbo.UserTypes", "Type");
            DropColumn("dbo.ProjectUserTypes", "AliasDescription");
            DropColumn("dbo.ProjectUserTypes", "AliasType");
            DropColumn("dbo.ProjectUserTypes", "AliasCode");
        }
    }
}
