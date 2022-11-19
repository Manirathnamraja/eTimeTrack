namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTypes3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProjectUserTypes", "Employee_Id", "dbo.Employees");
            DropIndex("dbo.ProjectUserTypes", new[] { "Employee_Id" });
            DropColumn("dbo.ProjectUserTypes", "Employee_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProjectUserTypes", "Employee_Id", c => c.Int());
            CreateIndex("dbo.ProjectUserTypes", "Employee_Id");
            AddForeignKey("dbo.ProjectUserTypes", "Employee_Id", "dbo.Employees", "EmployeeID");
        }
    }
}
