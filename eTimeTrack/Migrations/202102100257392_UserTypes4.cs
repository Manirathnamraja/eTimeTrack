namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTypes4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTypes", "Code", c => c.String());
            DropColumn("dbo.UserTypes", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserTypes", "Name", c => c.String());
            DropColumn("dbo.UserTypes", "Code");
        }
    }
}
