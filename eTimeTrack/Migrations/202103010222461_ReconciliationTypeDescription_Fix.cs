namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconciliationTypeDescription_Fix : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.ReconciliationTypes", "Description", c => c.String(maxLength: 511));
        }
        
        public override void Down()
        {
            //DropColumn("dbo.ReconciliationTypes", "Description");
        }
    }
}
