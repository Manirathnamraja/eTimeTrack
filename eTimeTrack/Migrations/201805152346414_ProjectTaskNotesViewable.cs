namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectTaskNotesViewable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "DisplayTaskNotes", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Projects", "DisplayTaskNotes");
        }
    }
}
