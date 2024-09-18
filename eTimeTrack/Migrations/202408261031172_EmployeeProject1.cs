namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeProject1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.EmployeeProjects", "ProjectDisciplineID");
            DropColumn("dbo.EmployeeProjects", "OfficeID");
            DropColumn("dbo.EmployeeProjects", "ProjectRole");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EmployeeProjects", "ProjectRole", c => c.String());
            AddColumn("dbo.EmployeeProjects", "OfficeID", c => c.Int());
            AddColumn("dbo.EmployeeProjects", "ProjectDisciplineID", c => c.Int());
        }
    }
}
