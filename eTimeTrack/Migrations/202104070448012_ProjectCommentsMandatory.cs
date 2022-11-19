namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectCommentsMandatory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "CommentsMandatory", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Projects", "CommentsMandatory");
        }
    }
}
