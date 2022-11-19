namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveFinancialPeriods : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FinancialPeriods", "LastModifiedBy", "dbo.Employees");
            DropForeignKey("dbo.TimesheetPeriods", "FinancialPeriodID", "dbo.FinancialPeriods");
            DropIndex("dbo.TimesheetPeriods", new[] { "FinancialPeriodID" });
            DropIndex("dbo.FinancialPeriods", new[] { "LastModifiedBy" });
            DropColumn("dbo.TimesheetPeriods", "FinancialPeriodID");
            DropTable("dbo.FinancialPeriods");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.FinancialPeriods",
                c => new
                    {
                        FinancialPeriodID = c.Int(nullable: false, identity: true),
                        Year = c.Int(nullable: false),
                        Code = c.String(maxLength: 10),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        DisplayMonth = c.DateTime(),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.FinancialPeriodID);
            
            AddColumn("dbo.TimesheetPeriods", "FinancialPeriodID", c => c.Int(nullable: false));
            CreateIndex("dbo.FinancialPeriods", "LastModifiedBy");
            CreateIndex("dbo.TimesheetPeriods", "FinancialPeriodID");
            AddForeignKey("dbo.TimesheetPeriods", "FinancialPeriodID", "dbo.FinancialPeriods", "FinancialPeriodID");
            AddForeignKey("dbo.FinancialPeriods", "LastModifiedBy", "dbo.Employees", "EmployeeID");
        }
    }
}
