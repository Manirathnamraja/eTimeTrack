namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconciliationEntryComplete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReconciliationEntries", "Complete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReconciliationEntries", "Complete");
        }
    }
}
