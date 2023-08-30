namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectExpensesUploads", "TransactionID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectExpensesUploads", "TransactionID", c => c.Int());
        }
    }
}
