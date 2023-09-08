namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesUpload5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectExpensesUploads", "ProjectExpTypeID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectExpensesUploads", "ProjectExpTypeID");
        }
    }
}
