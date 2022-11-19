namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectTimesheetPeriods : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectTimesheetPeriods",
                c => new
                    {
                        ProjectID = c.Int(nullable: false),
                        TimesheetPeriodID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProjectID, t.TimesheetPeriodID })
                .ForeignKey("dbo.Projects", t => t.ProjectID)
                .ForeignKey("dbo.TimesheetPeriods", t => t.TimesheetPeriodID)
                .Index(t => t.ProjectID)
                .Index(t => t.TimesheetPeriodID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectTimesheetPeriods", "TimesheetPeriodID", "dbo.TimesheetPeriods");
            DropForeignKey("dbo.ProjectTimesheetPeriods", "ProjectID", "dbo.Projects");
            DropIndex("dbo.ProjectTimesheetPeriods", new[] { "TimesheetPeriodID" });
            DropIndex("dbo.ProjectTimesheetPeriods", new[] { "ProjectID" });
            DropTable("dbo.ProjectTimesheetPeriods");
        }
    }
}
