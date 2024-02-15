namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload9 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectExpensesUploads", "ExpenseDate", c => c.String());
            AlterColumn("dbo.ProjectExpensesUploads", "Cost", c => c.String());
            AlterColumn("dbo.ProjectExpensesUploads", "EmployeeSupplierName", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectExpensesUploads", "EmployeeSupplierName", c => c.String(nullable: false));
            AlterColumn("dbo.ProjectExpensesUploads", "Cost", c => c.String(nullable: false));
            AlterColumn("dbo.ProjectExpensesUploads", "ExpenseDate", c => c.String(nullable: false));
        }
    }
}
