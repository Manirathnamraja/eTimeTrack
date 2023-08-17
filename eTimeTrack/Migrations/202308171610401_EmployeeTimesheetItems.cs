namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeTimesheetItems : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmployeeTimesheetItems", "IsTimeSheetApproval", c => c.Boolean());
            AddColumn("dbo.EmployeeTimesheetItems", "Reviewercomments", c => c.String());
            AddColumn("dbo.EmployeeTimesheetItems", "LastApprovedBy", c => c.Int());
            AddColumn("dbo.EmployeeTimesheetItems", "LastApprovedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmployeeTimesheetItems", "LastApprovedDate");
            DropColumn("dbo.EmployeeTimesheetItems", "LastApprovedBy");
            DropColumn("dbo.EmployeeTimesheetItems", "Reviewercomments");
            DropColumn("dbo.EmployeeTimesheetItems", "IsTimeSheetApproval");
        }
    }
}
