namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconciliationNullType : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ReconciliationEntries", new[] { "ReconciliationTypeId" });
            AlterColumn("dbo.ReconciliationEntries", "ReconciliationTypeId", c => c.Int());
            CreateIndex("dbo.ReconciliationEntries", "ReconciliationTypeId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ReconciliationEntries", new[] { "ReconciliationTypeId" });
            AlterColumn("dbo.ReconciliationEntries", "ReconciliationTypeId", c => c.Int(nullable: false));
            CreateIndex("dbo.ReconciliationEntries", "ReconciliationTypeId");
        }
    }
}
