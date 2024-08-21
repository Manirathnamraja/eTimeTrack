namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnassignedEmployeeProjects1 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.UnassignedEmployeeProjects", "IX_EmployeeProjectRestraint");
            CreateIndex("dbo.UnassignedEmployeeProjects", "EmployeeId");
            CreateIndex("dbo.UnassignedEmployeeProjects", "ProjectId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UnassignedEmployeeProjects", new[] { "ProjectId" });
            DropIndex("dbo.UnassignedEmployeeProjects", new[] { "EmployeeId" });
            CreateIndex("dbo.UnassignedEmployeeProjects", new[] { "EmployeeId", "ProjectId" }, unique: true, name: "IX_EmployeeProjectRestraint");
        }
    }
}
