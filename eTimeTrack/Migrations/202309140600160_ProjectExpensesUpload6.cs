namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectExpensesUploads", "VariationID", c => c.Int(nullable: false));
            AddColumn("dbo.ProjectExpensesUploads", "TaskID", c => c.Int(nullable: false));
            AddColumn("dbo.ProjectExpensesUploads", "IsFeeRecovery", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProjectExpensesUploads", "IsCostRecovery", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProjectExpensesUploads", "Completed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectExpensesUploads", "Completed");
            DropColumn("dbo.ProjectExpensesUploads", "IsCostRecovery");
            DropColumn("dbo.ProjectExpensesUploads", "IsFeeRecovery");
            DropColumn("dbo.ProjectExpensesUploads", "TaskID");
            DropColumn("dbo.ProjectExpensesUploads", "VariationID");
        }
    }
}
