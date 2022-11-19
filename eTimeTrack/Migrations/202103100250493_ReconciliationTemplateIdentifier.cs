namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconciliationTemplateIdentifier : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReconciliationTemplates", "TypeIdentifierColumn", c => c.String(maxLength: 2));
            AddColumn("dbo.ReconciliationTemplates", "TypeIdentifierText", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReconciliationTemplates", "TypeIdentifierText");
            DropColumn("dbo.ReconciliationTemplates", "TypeIdentifierColumn");
        }
    }
}
