namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeProjectJoin : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmployeeProjects",
                c => new
                    {
                        EmployeeProjectId = c.Int(nullable: false, identity: true),
                        EmployeeId = c.Int(nullable: false),
                        ProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EmployeeProjectId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .Index(t => new { t.EmployeeId, t.ProjectId }, unique: true, name: "IX_EmployeeProjectRestraint");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmployeeProjects", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.EmployeeProjects", "EmployeeId", "dbo.Employees");
            DropIndex("dbo.EmployeeProjects", "IX_EmployeeProjectRestraint");
            DropTable("dbo.EmployeeProjects");
        }
    }
}
