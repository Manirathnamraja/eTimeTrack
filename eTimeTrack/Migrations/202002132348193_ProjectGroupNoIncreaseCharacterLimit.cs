namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectGroupNoIncreaseCharacterLimit : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectGroups", "GroupNo", c => c.String(nullable: false, maxLength: 12));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectGroups", "GroupNo", c => c.String(nullable: false, maxLength: 10));
        }
    }
}
