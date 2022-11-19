namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableHours : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.EmployeeTimesheetItems", "Day1Hrs", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day2Hrs", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day3Hrs", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day4Hrs", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day5Hrs", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day6Hrs", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day7Hrs", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.EmployeeTimesheetItems", "Day7Hrs", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day6Hrs", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day5Hrs", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day4Hrs", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day3Hrs", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day2Hrs", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.EmployeeTimesheetItems", "Day1Hrs", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
