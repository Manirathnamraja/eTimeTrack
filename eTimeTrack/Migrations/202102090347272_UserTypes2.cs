namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTypes2 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ProjectUserTypes", new[] { "UserTypeID" });
            AlterColumn("dbo.ProjectUserTypes", "UserTypeID", c => c.Int());
            CreateIndex("dbo.ProjectUserTypes", "UserTypeID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ProjectUserTypes", new[] { "UserTypeID" });
            AlterColumn("dbo.ProjectUserTypes", "UserTypeID", c => c.Int(nullable: false));
            CreateIndex("dbo.ProjectUserTypes", "UserTypeID");
        }
    }
}
