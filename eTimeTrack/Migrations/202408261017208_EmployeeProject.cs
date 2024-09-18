namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeProject : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EmployeeProjects", "ProjectDisciplineID", "dbo.ProjectDisciplines");
            DropIndex("dbo.EmployeeProjects", new[] { "ProjectDisciplineID" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.EmployeeProjects", "ProjectDisciplineID");
            AddForeignKey("dbo.EmployeeProjects", "ProjectDisciplineID", "dbo.ProjectDisciplines", "ProjectDisciplineId");
        }
    }
}
