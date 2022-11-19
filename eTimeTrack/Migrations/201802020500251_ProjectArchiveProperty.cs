namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectArchiveProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "IsArchived", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Projects", "IsArchived");
        }
    }
}
