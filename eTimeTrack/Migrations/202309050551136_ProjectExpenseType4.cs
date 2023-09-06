namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpenseType4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectExpenseTypes", "IsFeeRecovery", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProjectExpenseTypes", "IsCostRecovery", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectExpenseTypes", "IsCostRecovery");
            DropColumn("dbo.ProjectExpenseTypes", "IsFeeRecovery");
        }
    }
}
