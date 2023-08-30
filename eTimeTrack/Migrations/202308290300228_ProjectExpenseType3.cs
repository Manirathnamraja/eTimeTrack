namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpenseType3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectExpenseTypes", "VariationID", c => c.Int(nullable: false));
            AlterColumn("dbo.ProjectExpenseTypes", "TaskID", c => c.Int(nullable: false));
            AlterColumn("dbo.ProjectExpenseTypes", "ProjectID", c => c.Int(nullable: false));
            AlterColumn("dbo.ProjectExpenseTypes", "IsClosed", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ProjectExpenseTypes", "LastModifiedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectExpenseTypes", "LastModifiedDate", c => c.String());
            AlterColumn("dbo.ProjectExpenseTypes", "IsClosed", c => c.Boolean());
            AlterColumn("dbo.ProjectExpenseTypes", "ProjectID", c => c.Int());
            AlterColumn("dbo.ProjectExpenseTypes", "TaskID", c => c.Int());
            AlterColumn("dbo.ProjectExpenseTypes", "VariationID", c => c.Int());
        }
    }
}
