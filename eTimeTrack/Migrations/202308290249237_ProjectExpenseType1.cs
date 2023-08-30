namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpenseType1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectExpenseTypes", "IsClosed", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProjectExpenseTypes", "LastModifiedBy", c => c.Int());
            AddColumn("dbo.ProjectExpenseTypes", "LastModifiedDate", c => c.String());
            AlterColumn("dbo.ProjectExpenseTypes", "VariationID", c => c.Int());
            AlterColumn("dbo.ProjectExpenseTypes", "TaskID", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectExpenseTypes", "TaskID", c => c.Int(nullable: false));
            AlterColumn("dbo.ProjectExpenseTypes", "VariationID", c => c.Int(nullable: false));
            DropColumn("dbo.ProjectExpenseTypes", "LastModifiedDate");
            DropColumn("dbo.ProjectExpenseTypes", "LastModifiedBy");
            DropColumn("dbo.ProjectExpenseTypes", "IsClosed");
        }
    }
}
