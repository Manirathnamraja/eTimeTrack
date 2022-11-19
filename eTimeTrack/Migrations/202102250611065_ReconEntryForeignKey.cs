namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconEntryForeignKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReconciliationEntries", "EmployeeTimesheetId", c => c.Int());
            CreateIndex("dbo.ReconciliationEntries", "EmployeeTimesheetId");
            AddForeignKey("dbo.ReconciliationEntries", "EmployeeTimesheetId", "dbo.EmployeeTimesheets", "TimesheetID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReconciliationEntries", "EmployeeTimesheetId", "dbo.EmployeeTimesheets");
            DropIndex("dbo.ReconciliationEntries", new[] { "EmployeeTimesheetId" });
            DropColumn("dbo.ReconciliationEntries", "EmployeeTimesheetId");
        }
    }
}
