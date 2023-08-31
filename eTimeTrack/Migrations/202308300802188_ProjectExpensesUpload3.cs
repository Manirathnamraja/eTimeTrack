namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectExpensesUploads", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.ProjectExpensesUploads", "InvoiceNumber", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectExpensesUploads", "InvoiceNumber");
            DropColumn("dbo.ProjectExpensesUploads", "CompanyId");
        }
    }
}
