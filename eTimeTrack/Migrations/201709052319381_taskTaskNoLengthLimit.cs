namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class taskTaskNoLengthLimit : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectTasks", "TaskNo", c => c.String(nullable: false, maxLength: 30));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectTasks", "TaskNo", c => c.String(nullable: false));
        }
    }
}
