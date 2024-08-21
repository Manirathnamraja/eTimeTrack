namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnassignedEmployeeProjects : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UnassignedEmployeeProjects",
                c => new
                    {
                        EmployeeProjectId = c.Int(nullable: false, identity: true),
                        EmployeeId = c.Int(nullable: false),
                        ProjectId = c.Int(nullable: false),
                        ProjectUserTypeID = c.Int(),
                        ProjectDisciplineID = c.Int(),
                        OfficeID = c.Int(),
                        ProjectRole = c.String(),
                    })
                .PrimaryKey(t => t.EmployeeProjectId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .ForeignKey("dbo.ProjectDisciplines", t => t.ProjectDisciplineID)
                .ForeignKey("dbo.ProjectUserTypes", t => t.ProjectUserTypeID)
                .Index(t => new { t.EmployeeId, t.ProjectId }, unique: true, name: "IX_EmployeeProjectRestraint")
                .Index(t => t.ProjectUserTypeID)
                .Index(t => t.ProjectDisciplineID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UnassignedEmployeeProjects", "ProjectUserTypeID", "dbo.ProjectUserTypes");
            DropForeignKey("dbo.UnassignedEmployeeProjects", "ProjectDisciplineID", "dbo.ProjectDisciplines");
            DropForeignKey("dbo.UnassignedEmployeeProjects", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.UnassignedEmployeeProjects", "EmployeeId", "dbo.Employees");
            DropIndex("dbo.UnassignedEmployeeProjects", new[] { "ProjectDisciplineID" });
            DropIndex("dbo.UnassignedEmployeeProjects", new[] { "ProjectUserTypeID" });
            DropIndex("dbo.UnassignedEmployeeProjects", "IX_EmployeeProjectRestraint");
            DropTable("dbo.UnassignedEmployeeProjects");
        }
    }
}
