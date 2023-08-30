namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectExpensesUploads",
                c => new
                    {
                        ExpenseUploadID = c.Int(nullable: false, identity: true),
                        TransactionID = c.Int(),
                        ExpenseDate = c.DateTime(nullable: false),
                        CostedInWeekEnding = c.String(),
                        Cost = c.String(nullable: false),
                        HomeOfficeType = c.String(),
                        EmployeeSupplierName = c.String(nullable: false),
                        UOM = c.String(),
                        ExpenditureComment = c.String(),
                        ProjectComment = c.String(),
                        AddedBy = c.Int(),
                        AddedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ExpenseUploadID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProjectExpensesUploads");
        }
    }
}
