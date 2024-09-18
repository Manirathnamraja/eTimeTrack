namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeProjectDetails2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmployeeProjectDetails", "EmployeeId", c => c.Int(nullable: false));
            AddColumn("dbo.EmployeeProjectDetails", "ProjectId", c => c.Int(nullable: false));
            DropColumn("dbo.EmployeeProjectDetails", "EmployeeProjectId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EmployeeProjectDetails", "EmployeeProjectId", c => c.Int(nullable: false));
            DropColumn("dbo.EmployeeProjectDetails", "ProjectId");
            DropColumn("dbo.EmployeeProjectDetails", "EmployeeId");
        }
    }
}
