namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeProjectDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmployeeProjectDetails",
                c => new
                    {
                        EmployeeProjectDetailsID = c.Int(nullable: false, identity: true),
                        ProjectUserTypeID = c.Int(),
                        ProjectDisciplineID = c.Int(),
                        OfficeID = c.Int(),
                        ProjectRole = c.String(),
                    })
                .PrimaryKey(t => t.EmployeeProjectDetailsID)
                .ForeignKey("dbo.ProjectDisciplines", t => t.ProjectDisciplineID)
                .ForeignKey("dbo.ProjectUserTypes", t => t.ProjectUserTypeID)
                .Index(t => t.ProjectUserTypeID)
                .Index(t => t.ProjectDisciplineID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmployeeProjectDetails", "ProjectUserTypeID", "dbo.ProjectUserTypes");
            DropForeignKey("dbo.EmployeeProjectDetails", "ProjectDisciplineID", "dbo.ProjectDisciplines");
            DropIndex("dbo.EmployeeProjectDetails", new[] { "ProjectDisciplineID" });
            DropIndex("dbo.EmployeeProjectDetails", new[] { "ProjectUserTypeID" });
            DropTable("dbo.EmployeeProjectDetails");
        }
    }
}
