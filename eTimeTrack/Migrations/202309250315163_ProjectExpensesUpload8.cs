namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectExpensesUploads", "Traveller", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectExpensesUploads", "Traveller");
        }
    }
}
