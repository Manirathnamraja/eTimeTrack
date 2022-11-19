namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconTemplateDailyDates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReconciliationTemplates", "DailyDates", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReconciliationTemplates", "DailyDates");
        }
    }
}
