namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpenseType2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProjectExpenseTypes", "ProjectID", c => c.Int());
            AlterColumn("dbo.ProjectExpenseTypes", "IsClosed", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProjectExpenseTypes", "IsClosed", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ProjectExpenseTypes", "ProjectID", c => c.Int(nullable: false));
        }
    }
}
