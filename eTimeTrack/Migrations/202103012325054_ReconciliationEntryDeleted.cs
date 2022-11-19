namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconciliationEntryDeleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReconciliationEntries", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReconciliationEntries", "Deleted");
        }
    }
}