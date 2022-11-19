namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SystemEventsLockout : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SystemEvents",
                c => new
                    {
                        SystemEventId = c.Int(nullable: false, identity: true),
                        EventTitle = c.String(nullable: false, maxLength: 50),
                        DateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.SystemEventId);
            
            AddColumn("dbo.Employees", "LockoutDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "LockoutDate");
            DropTable("dbo.SystemEvents");
        }
    }
}
