namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SystemEventsLockout_UpdatedName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "LockoutDateTimeUtc", c => c.DateTime());
            DropColumn("dbo.Employees", "LockoutDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Employees", "LockoutDate", c => c.DateTime());
            DropColumn("dbo.Employees", "LockoutDateTimeUtc");
        }
    }
}
