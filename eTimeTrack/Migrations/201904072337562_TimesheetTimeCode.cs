namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TimesheetTimeCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmployeeTimesheetItems", "TimeCode", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmployeeTimesheetItems", "TimeCode");
        }
    }
}
