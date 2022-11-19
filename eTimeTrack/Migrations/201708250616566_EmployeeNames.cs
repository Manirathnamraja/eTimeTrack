namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeNames : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Names", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "Names");
        }
    }
}
