namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload4 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectExpensesUploads", "InvoiceNumber", c => c.String());
            AlterColumn("dbo.ProjectExpensesUploads", "ExpenseDate", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectExpensesUploads", "ExpenseDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ProjectExpensesUploads", "InvoiceNumber", c => c.Int());
        }
    }
}
