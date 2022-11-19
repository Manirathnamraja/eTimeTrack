namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeCommentOnReconEntry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReconciliationEntries", "EmployeeComment", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReconciliationEntries", "EmployeeComment");
        }
    }
}
